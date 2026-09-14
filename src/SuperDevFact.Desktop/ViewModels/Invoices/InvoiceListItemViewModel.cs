using SuperDevFact.Application.Invoices;

namespace SuperDevFact.Desktop.ViewModels.Invoices;

/// <summary>Enveloppe d'affichage : combine le statut technique et le caractère "en retard" (calculé) en un seul statut affichable.</summary>
public sealed class InvoiceListItemViewModel(InvoiceSummaryDto dto)
{
    public Guid Id { get; } = dto.Id;
    public string Number { get; } = dto.Number;
    public string CustomerName { get; } = dto.CustomerName;
    public DateOnly IssueDate { get; } = dto.IssueDate;
    public decimal TotalTtc { get; } = dto.TotalTtc;
    public string DisplayStatus { get; } = dto.IsOverdue ? "Overdue" : dto.Status;
}
