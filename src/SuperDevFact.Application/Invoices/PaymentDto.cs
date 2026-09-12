namespace SuperDevFact.Application.Invoices;

public sealed record PaymentDto(Guid Id, decimal Amount, DateOnly PaymentDate, string Method, string? Reference);
