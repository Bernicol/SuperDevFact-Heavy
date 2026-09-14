using System.Collections.ObjectModel;
using Material.Icons;
using SuperDevFact.Application.Quotes;
using SuperDevFact.Desktop.Infrastructure;

namespace SuperDevFact.Desktop.ViewModels.Quotes;

/// <summary>
/// Liste de tous les devis, regroupés en entonnoir (Brouillons → Envoyés → Acceptés →
/// Facturés → Refusés/expirés) plutôt qu'en une seule table plate : pensé pour un usage
/// quotidien, où l'étape du pipeline est l'information la plus utile pour prioriser.
/// </summary>
public sealed class QuoteListViewModel(ScopedUseCaseRunner runner) : ViewModelBase
{
    public event Action<QuoteSummaryDto>? QuoteSelected;

    public ObservableCollection<QuoteStatItemViewModel> Stats { get; } = new();

    public ObservableCollection<QuoteGroupViewModel> Groups { get; } = new();

    private RelayCommand? _selectQuoteCommand;
    public RelayCommand SelectQuote => _selectQuoteCommand ??= new RelayCommand(p =>
    {
        if (p is QuoteSummaryDto quote)
            QuoteSelected?.Invoke(quote);
    });

    public async Task InitializeAsync()
    {
        var quotes = await runner.RunAsync<SearchQuotesUseCase, IReadOnlyList<QuoteSummaryDto>>(uc => uc.ExecuteAsync(null, null));

        QuoteSummaryDto[] Bucket(Func<QuoteSummaryDto, bool> predicate) =>
            quotes.Where(predicate).OrderByDescending(q => q.IssueDate).ToArray();

        var draft = Bucket(q => q.Status == "Draft");
        var sent = Bucket(q => q.Status == "Sent");
        var accepted = Bucket(q => q.Status == "Accepted" && q.ConvertedInvoiceId is null);
        var converted = Bucket(q => q.ConvertedInvoiceId is not null);
        var closed = Bucket(q => q.Status is "Declined" or "Expired");

        Stats.Clear();
        Stats.Add(new QuoteStatItemViewModel("Brouillons", MaterialIconKind.FileDocumentOutline, "#94A3B8", draft.Length, draft.Sum(q => q.TotalHt)));
        Stats.Add(new QuoteStatItemViewModel("Envoyés", MaterialIconKind.SendOutline, "#3B82F6", sent.Length, sent.Sum(q => q.TotalHt)));
        Stats.Add(new QuoteStatItemViewModel("Acceptés", MaterialIconKind.CheckCircleOutline, "#34D399", accepted.Length, accepted.Sum(q => q.TotalHt)));
        Stats.Add(new QuoteStatItemViewModel("Facturés", MaterialIconKind.ReceiptTextOutline, "#A78BFA", converted.Length, converted.Sum(q => q.TotalHt)));
        Stats.Add(new QuoteStatItemViewModel("Refusés/expirés", MaterialIconKind.CloseCircleOutline, "#FB7185", closed.Length, closed.Sum(q => q.TotalHt)));

        Groups.Clear();
        if (draft.Length > 0)
            Groups.Add(new QuoteGroupViewModel("Brouillons", "À finaliser et envoyer", MaterialIconKind.FileDocumentOutline, "#94A3B8", draft));
        if (sent.Length > 0)
            Groups.Add(new QuoteGroupViewModel("Envoyés", "En attente de réponse client", MaterialIconKind.SendOutline, "#3B82F6", sent));
        if (accepted.Length > 0)
            Groups.Add(new QuoteGroupViewModel("Acceptés", "Prêts à transformer en facture", MaterialIconKind.CheckCircleOutline, "#34D399", accepted));
        if (converted.Length > 0)
            Groups.Add(new QuoteGroupViewModel("Facturés", "Déjà transformés en facture", MaterialIconKind.ReceiptTextOutline, "#A78BFA", converted));
        if (closed.Length > 0)
            Groups.Add(new QuoteGroupViewModel("Refusés / expirés", "Sans suite", MaterialIconKind.CloseCircleOutline, "#FB7185", closed));
    }
}
