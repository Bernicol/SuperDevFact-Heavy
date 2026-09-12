using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Domain.Invoices;

/// <summary>
/// Facture : agrégat racine. Dans ce prototype, une facture nait toujours de la
/// transformation d'un devis accepté (cf. <see cref="CreateFromQuote"/>) : la création
/// de facture "à blanc" n'est pas couverte par le vertical slice, mais l'architecture
/// n'empêche pas de l'ajouter plus tard via un second constructeur nommé.
/// </summary>
public sealed class Invoice : FinancialDocument<InvoiceLine>
{
    private readonly List<Payment> _payments = new();

    public Guid Id { get; }
    public InvoiceNumber Number { get; }
    public Guid CustomerId { get; private set; }
    public Guid? SourceQuoteId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }
    public string? ClientReference { get; private set; }
    public string? InternalNotes { get; private set; }
    public string? ClientMessage { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset LastModifiedAtUtc { get; private set; }

    public IReadOnlyList<Payment> Payments => _payments;

    private Invoice()
    {
        PaymentTerms = PaymentTerms.Immediate;
    } // réservé à EF Core

    private Invoice(
        InvoiceNumber number,
        Guid customerId,
        Guid? sourceQuoteId,
        DateOnly issueDate,
        PaymentTerms paymentTerms,
        string? clientReference,
        string? internalNotes,
        string? clientMessage)
    {
        Id = Guid.NewGuid();
        Number = number;
        CustomerId = customerId;
        SourceQuoteId = sourceQuoteId;
        IssueDate = issueDate;
        PaymentTerms = paymentTerms with { }; // clone défensif, cf. DocumentLine.SetContent
        DueDate = paymentTerms.ComputeDueDate(issueDate);
        ClientReference = clientReference;
        InternalNotes = internalNotes;
        ClientMessage = clientMessage;
        Status = InvoiceStatus.Sent;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        LastModifiedAtUtc = CreatedAtUtc;
    }

    /// <summary>
    /// Cas d'usage central du prototype : transforme un devis accepté en facture.
    /// Copie fidèlement les lignes et la remise globale du devis au moment de la
    /// conversion (la facture ne doit plus évoluer si le devis change ensuite).
    /// </summary>
    public static Invoice CreateFromQuote(Quote quote, InvoiceNumber number, DateOnly issueDate)
    {
        if (quote is null) throw new ArgumentNullException(nameof(quote));

        if (quote.Status != QuoteStatus.Accepted)
            throw new InvalidOperationException("Seul un devis accepté peut être transformé en facture.");

        if (quote.ConvertedInvoiceId is not null)
            throw new InvalidOperationException("Ce devis a déjà été transformé en facture.");

        var invoice = new Invoice(
            number,
            quote.CustomerId,
            quote.Id,
            issueDate,
            quote.PaymentTerms,
            quote.ClientReference,
            quote.InternalNotes,
            quote.ClientMessage)
        {
            GlobalDiscountRate = quote.GlobalDiscountRate
        };

        foreach (var line in quote.Lines)
        {
            invoice.RegisterLine(new InvoiceLine(
                line.Description, line.Detail, line.Quantity, line.Unit, line.UnitPriceHt, line.DiscountRate, line.TaxRate));
        }

        return invoice;
    }

    public Money AmountPaid(string currency = Money.DefaultCurrency) =>
        _payments.Aggregate(Money.Zero(currency), static (acc, p) => acc + p.Amount);

    public Money Balance(string currency = Money.DefaultCurrency) =>
        CalculateTotals(currency).TotalTtc - AmountPaid(currency);

    public void RecordPayment(Money amount, DateOnly paymentDate, PaymentMethod method, string? reference = null)
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Impossible d'enregistrer un paiement sur une facture annulée.");

        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cette facture est déjà intégralement payée.");

        var payment = new Payment(Id, amount, paymentDate, method, reference);
        _payments.Add(payment);
        Touch();

        var remaining = Balance(amount.Currency);
        Status = remaining.Amount <= 0m ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Une facture intégralement payée ne peut pas être annulée.");

        Status = InvoiceStatus.Cancelled;
        Touch();
    }

    /// <summary>
    /// L'échéance dépassée n'est pas un état stocké : elle est calculée à la demande
    /// par rapport à une date de référence (généralement "aujourd'hui").
    /// </summary>
    public bool IsOverdue(DateOnly asOfDate) =>
        Status is InvoiceStatus.Sent or InvoiceStatus.PartiallyPaid && asOfDate > DueDate;

    private void Touch() => LastModifiedAtUtc = DateTimeOffset.UtcNow;
}
