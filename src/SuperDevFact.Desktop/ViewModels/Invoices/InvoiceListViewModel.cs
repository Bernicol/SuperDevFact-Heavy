using System.Collections.ObjectModel;
using Material.Icons;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Desktop.Infrastructure;

namespace SuperDevFact.Desktop.ViewModels.Invoices;

/// <summary>
/// Liste de toutes les factures, regroupées par ordre de priorité opérationnelle : ce qui
/// réclame une action en premier (retards), puis le recouvrement en cours, puis le soldé.
/// </summary>
public sealed class InvoiceListViewModel(ScopedUseCaseRunner runner) : ViewModelBase
{
    public event Action<Guid>? InvoiceSelected;

    public ObservableCollection<InvoiceStatItemViewModel> Stats { get; } = new();

    public ObservableCollection<InvoiceGroupViewModel> Groups { get; } = new();

    private RelayCommand? _selectInvoiceCommand;
    public RelayCommand SelectInvoice => _selectInvoiceCommand ??= new RelayCommand(p =>
    {
        if (p is InvoiceListItemViewModel invoice)
            InvoiceSelected?.Invoke(invoice.Id);
    });

    public async Task InitializeAsync()
    {
        var invoices = (await runner.RunAsync<SearchInvoicesUseCase, IReadOnlyList<InvoiceSummaryDto>>(uc => uc.ExecuteAsync(null, null)))
            .OrderByDescending(i => i.IssueDate)
            .Select(i => new InvoiceListItemViewModel(i))
            .ToList();

        InvoiceListItemViewModel[] Bucket(Func<InvoiceListItemViewModel, bool> predicate) => invoices.Where(predicate).ToArray();

        var overdue = Bucket(i => i.DisplayStatus == "Overdue");
        var partiallyPaid = Bucket(i => i.DisplayStatus == "PartiallyPaid");
        var pending = Bucket(i => i.DisplayStatus == "Sent");
        var paid = Bucket(i => i.DisplayStatus == "Paid");

        Stats.Clear();
        Stats.Add(new InvoiceStatItemViewModel("En retard", MaterialIconKind.AlertCircleOutline, "#FB7185", overdue.Length, overdue.Sum(i => i.TotalTtc)));
        Stats.Add(new InvoiceStatItemViewModel("Partiellement payées", MaterialIconKind.CashClock, "#FBBF24", partiallyPaid.Length, partiallyPaid.Sum(i => i.TotalTtc)));
        Stats.Add(new InvoiceStatItemViewModel("En attente", MaterialIconKind.ClockOutline, "#3B82F6", pending.Length, pending.Sum(i => i.TotalTtc)));
        Stats.Add(new InvoiceStatItemViewModel("Payées", MaterialIconKind.CashCheck, "#34D399", paid.Length, paid.Sum(i => i.TotalTtc)));

        Groups.Clear();
        if (overdue.Length > 0)
            Groups.Add(new InvoiceGroupViewModel("En retard", "Échéance dépassée — à relancer", MaterialIconKind.AlertCircleOutline, "#FB7185", overdue));
        if (partiallyPaid.Length > 0)
            Groups.Add(new InvoiceGroupViewModel("Partiellement payées", "Solde restant à encaisser", MaterialIconKind.CashClock, "#FBBF24", partiallyPaid));
        if (pending.Length > 0)
            Groups.Add(new InvoiceGroupViewModel("En attente", "Envoyées, dans les délais", MaterialIconKind.ClockOutline, "#3B82F6", pending));
        if (paid.Length > 0)
            Groups.Add(new InvoiceGroupViewModel("Payées", "Soldées", MaterialIconKind.CashCheck, "#34D399", paid));
    }
}
