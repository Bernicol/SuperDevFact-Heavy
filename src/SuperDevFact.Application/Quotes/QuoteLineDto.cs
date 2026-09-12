namespace SuperDevFact.Application.Quotes;

public sealed record QuoteLineDto(
    Guid Id,
    int Position,
    string Description,
    string? Detail,
    decimal Quantity,
    string Unit,
    decimal UnitPriceHt,
    decimal DiscountRatePercent,
    decimal TaxRatePercent,
    string TaxLabel,
    decimal LineTotalHt);
