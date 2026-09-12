using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Application.Invoices;

public sealed record RecordPaymentRequest(decimal Amount, DateOnly PaymentDate, PaymentMethod Method, string? Reference);
