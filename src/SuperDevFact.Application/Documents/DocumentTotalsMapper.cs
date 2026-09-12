using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Application.Documents;

public static class DocumentTotalsMapper
{
    public static DocumentTotalsDto ToDto(DocumentTotals totals) => new(
        totals.SubtotalHt.Amount,
        totals.GlobalDiscountAmount.Amount,
        totals.NetHt.Amount,
        totals.TaxBreakdown
            .Select(b => new TaxBreakdownDto(b.TaxRate.Label, b.TaxRate.Rate.Value, b.TaxableBase.Amount, b.TaxAmount.Amount))
            .ToList(),
        totals.TotalTax.Amount,
        totals.TotalTtc.Amount);
}
