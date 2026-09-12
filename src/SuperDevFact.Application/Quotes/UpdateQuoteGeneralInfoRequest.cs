namespace SuperDevFact.Application.Quotes;

public sealed record UpdateQuoteGeneralInfoRequest(
    Guid CustomerId,
    DateOnly IssueDate,
    int ValidityDays,
    int PaymentTermsDays,
    string? ClientReference);
