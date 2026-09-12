using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Domain.Invoices;

public sealed class InvoiceLine : DocumentLine
{
    private InvoiceLine() : base("_", null, 1m, "_", Money.Zero(), Percentage.Zero, TaxRate.Standard)
    {
        // Réservé à EF Core ; le contenu réel est réhydraté par le mapping.
    }

    public InvoiceLine(
        string description,
        string? detail,
        decimal quantity,
        string unit,
        Money unitPriceHt,
        Percentage discountRate,
        TaxRate taxRate)
        : base(description, detail, quantity, unit, unitPriceHt, discountRate, taxRate)
    {
    }
}
