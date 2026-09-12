namespace SuperDevFact.Application.Quotes;

/// <summary>Représentation légère d'un devis pour les listes (dashboard, recherche, command palette).</summary>
public sealed record QuoteSummaryDto(
    Guid Id,
    string Number,
    string CustomerName,
    DateOnly IssueDate,
    decimal TotalHt,
    string Status);
