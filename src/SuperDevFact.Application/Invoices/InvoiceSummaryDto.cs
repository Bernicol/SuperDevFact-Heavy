namespace SuperDevFact.Application.Invoices;

/// <summary>Représentation légère d'une facture pour les listes (dashboard, recherche, command palette).</summary>
public sealed record InvoiceSummaryDto(
    Guid Id,
    string Number,
    string CustomerName,
    DateOnly IssueDate,
    decimal TotalTtc,
    string Status,
    bool IsOverdue);
