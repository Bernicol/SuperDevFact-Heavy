using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Documents;

namespace SuperDevFact.Application.Invoices;

public sealed record InvoiceDetailsDto(
    Guid Id,
    string Number,
    CustomerSummaryDto Customer,
    Guid? SourceQuoteId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string PaymentTermsLabel,
    string? ClientReference,
    string? InternalNotes,
    string? ClientMessage,
    string Status,
    bool IsOverdue,
    decimal GlobalDiscountRatePercent,
    IReadOnlyList<InvoiceLineDto> Lines,
    IReadOnlyList<PaymentDto> Payments,
    decimal AmountPaid,
    decimal Balance,
    DocumentTotalsDto Totals,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset LastModifiedAtUtc);
