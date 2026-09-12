namespace SuperDevFact.Application.Quotes;

public sealed record QuoteLineRequest(
    string Description,
    string? Detail,
    decimal Quantity,
    string Unit,
    decimal UnitPriceHt,
    decimal DiscountRatePercent,
    decimal TaxRatePercent);
