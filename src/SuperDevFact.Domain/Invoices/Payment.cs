using SuperDevFact.Domain.Common;

namespace SuperDevFact.Domain.Invoices;

/// <summary>
/// Encaissement rattaché à une facture. La construction est réservée à l'agrégat
/// <see cref="Invoice"/> : un paiement n'a pas de sens hors du contexte de la facture
/// qu'il solde, partiellement ou totalement.
/// </summary>
public sealed class Payment
{
    public Guid Id { get; }
    public Guid InvoiceId { get; }
    public Money Amount { get; }
    public DateOnly PaymentDate { get; }
    public PaymentMethod Method { get; }
    public string? Reference { get; }

    private Payment()
    {
        Amount = Money.Zero();
    } // réservé à EF Core

    internal Payment(Guid invoiceId, Money amount, DateOnly paymentDate, PaymentMethod method, string? reference)
    {
        if (amount.Amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Le montant d'un paiement doit être positif.");

        Id = Guid.NewGuid();
        InvoiceId = invoiceId;
        Amount = amount;
        PaymentDate = paymentDate;
        Method = method;
        Reference = reference;
    }
}
