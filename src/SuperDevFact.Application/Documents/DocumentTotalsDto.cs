namespace SuperDevFact.Application.Documents;

public sealed record TaxBreakdownDto(string Label, decimal RatePercent, decimal TaxableBase, decimal TaxAmount);

public sealed record DocumentTotalsDto(
    decimal SubtotalHt,
    decimal GlobalDiscountAmount,
    decimal NetHt,
    IReadOnlyList<TaxBreakdownDto> TaxBreakdown,
    decimal TotalTax,
    decimal TotalTtc);
