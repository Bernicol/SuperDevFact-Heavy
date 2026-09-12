namespace SuperDevFact.Application.Invoices;

public sealed record InvoiceLineDto(
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
