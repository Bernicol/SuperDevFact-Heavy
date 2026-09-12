namespace SuperDevFact.Application.Quotes;

public sealed record CreateQuoteRequest(
    Guid CustomerId,
    DateOnly? IssueDate = null,
    int ValidityDays = 30,
    int? PaymentTermsDays = null,
    string? ClientReference = null);
