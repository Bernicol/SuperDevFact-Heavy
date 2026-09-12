using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Domain.Quotes;

/// <summary>
/// Devis : agrégat racine. Les lignes ne sont modifiables que tant que le devis est
/// en brouillon — une fois envoyé, le document doit rester fidèle à ce qui a été
/// communiqué au client.
/// </summary>
public sealed class Quote : FinancialDocument<QuoteLine>
{
    public Guid Id { get; }
    public QuoteNumber Number { get; }
    public Guid CustomerId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public int ValidityDays { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }
    public string? ClientReference { get; private set; }
    public string? InternalNotes { get; private set; }
    public string? ClientMessage { get; private set; }
    public QuoteStatus Status { get; private set; }
    public Guid? ConvertedInvoiceId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset LastModifiedAtUtc { get; private set; }

    public DateOnly ExpiryDate => IssueDate.AddDays(ValidityDays);

    private Quote()
    {
        PaymentTerms = PaymentTerms.Immediate;
    } // réservé à EF Core

    public Quote(
        QuoteNumber number,
        Guid customerId,
        DateOnly issueDate,
        int validityDays,
        PaymentTerms paymentTerms,
        string? clientReference = null)
    {
        if (validityDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(validityDays), validityDays, "La validité doit être d'au moins un jour.");

        Id = Guid.NewGuid();
        Number = number;
        CustomerId = customerId;
        IssueDate = issueDate;
        ValidityDays = validityDays;
        PaymentTerms = paymentTerms with { }; // clone défensif, cf. DocumentLine.SetContent
        ClientReference = clientReference;
        Status = QuoteStatus.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        LastModifiedAtUtc = CreatedAtUtc;
    }

    public QuoteLine AddLine(
        string description,
        string? detail,
        decimal quantity,
        string unit,
        Money unitPriceHt,
        Percentage discountRate,
        TaxRate taxRate)
    {
        EnsureEditable();

        var line = new QuoteLine(description, detail, quantity, unit, unitPriceHt, discountRate, taxRate);
        RegisterLine(line);
        Touch();
        return line;
    }

    public void UpdateLine(
        Guid lineId,
        string description,
        string? detail,
        decimal quantity,
        string unit,
        Money unitPriceHt,
        Percentage discountRate,
        TaxRate taxRate)
    {
        EnsureEditable();

        var line = GetLineOrThrow(lineId);
        line.SetContent(description, detail, quantity, unit, unitPriceHt, discountRate, taxRate);
        Touch();
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureEditable();
        RemoveLineInternal(lineId);
        Touch();
    }

    public void ReorderLines(IReadOnlyList<Guid> orderedLineIds)
    {
        EnsureEditable();
        ReorderLinesInternal(orderedLineIds);
        Touch();
    }

    public void ApplyGlobalDiscount(Percentage discountRate)
    {
        EnsureEditable();
        GlobalDiscountRate = discountRate;
        Touch();
    }

    public void UpdateGeneralInformation(
        Guid customerId,
        DateOnly issueDate,
        int validityDays,
        PaymentTerms paymentTerms,
        string? clientReference)
    {
        EnsureEditable();

        if (validityDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(validityDays), validityDays, "La validité doit être d'au moins un jour.");

        CustomerId = customerId;
        IssueDate = issueDate;
        ValidityDays = validityDays;
        PaymentTerms = paymentTerms with { }; // clone défensif, cf. DocumentLine.SetContent
        ClientReference = clientReference;
        Touch();
    }

    public void UpdateNotes(string? internalNotes, string? clientMessage)
    {
        InternalNotes = internalNotes;
        ClientMessage = clientMessage;
        Touch();
    }

    public void Send()
    {
        EnsureStatus(QuoteStatus.Draft, "Seul un devis en brouillon peut être envoyé.");
        if (Lines.Count == 0)
            throw new InvalidOperationException("Un devis sans ligne ne peut pas être envoyé.");

        Status = QuoteStatus.Sent;
        Touch();
    }

    public void Accept()
    {
        EnsureStatus(QuoteStatus.Sent, "Seul un devis envoyé peut être accepté.");
        Status = QuoteStatus.Accepted;
        Touch();
    }

    public void Decline()
    {
        EnsureStatus(QuoteStatus.Sent, "Seul un devis envoyé peut être refusé.");
        Status = QuoteStatus.Declined;
        Touch();
    }

    public void Expire()
    {
        if (Status is not (QuoteStatus.Draft or QuoteStatus.Sent))
            throw new InvalidOperationException("Seul un devis en brouillon ou envoyé peut expirer.");

        Status = QuoteStatus.Expired;
        Touch();
    }

    /// <summary>
    /// Marque le devis comme transformé en facture. Appelé par le cas d'usage
    /// ConvertQuoteToInvoice une fois la facture créée avec succès.
    /// </summary>
    public void MarkConverted(Guid invoiceId)
    {
        EnsureStatus(QuoteStatus.Accepted, "Seul un devis accepté peut être transformé en facture.");

        if (ConvertedInvoiceId is not null)
            throw new InvalidOperationException("Ce devis a déjà été transformé en facture.");

        ConvertedInvoiceId = invoiceId;
        Touch();
    }

    private void EnsureEditable()
    {
        if (Status != QuoteStatus.Draft)
            throw new InvalidOperationException("Seul un devis en brouillon peut être modifié.");
    }

    private void EnsureStatus(QuoteStatus expected, string errorMessage)
    {
        if (Status != expected)
            throw new InvalidOperationException(errorMessage);
    }

    private void Touch() => LastModifiedAtUtc = DateTimeOffset.UtcNow;
}
