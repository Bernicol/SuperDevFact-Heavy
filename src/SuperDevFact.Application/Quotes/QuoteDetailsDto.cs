using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Documents;

namespace SuperDevFact.Application.Quotes;

public sealed record QuoteDetailsDto(
    Guid Id,
    string Number,
    CustomerSummaryDto Customer,
    DateOnly IssueDate,
    DateOnly ExpiryDate,
    int ValidityDays,
    string PaymentTermsLabel,
    string? ClientReference,
    string? InternalNotes,
    string? ClientMessage,
    string Status,
    Guid? ConvertedInvoiceId,
    decimal GlobalDiscountRatePercent,
    IReadOnlyList<QuoteLineDto> Lines,
    DocumentTotalsDto Totals,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastModifiedAtUtc);
