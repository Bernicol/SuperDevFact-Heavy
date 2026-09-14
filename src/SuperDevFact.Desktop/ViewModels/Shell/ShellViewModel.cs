using Microsoft.Extensions.DependencyInjection;
using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Quotes;
using SuperDevFact.Desktop.Infrastructure;
using SuperDevFact.Desktop.ViewModels.Dashboard;
using SuperDevFact.Desktop.ViewModels.Invoices;
using SuperDevFact.Desktop.ViewModels.Quotes;

namespace SuperDevFact.Desktop.ViewModels.Shell;

/// <summary>
/// Navigation minimale : pas de framework, juste une propriété "vue courante" que la
/// fenêtre principale affiche via des DataTemplates par type de ViewModel. Chaque écran
/// de liste peut demander l'ouverture d'un devis ou d'une facture dans l'atelier
/// (QuoteEditorViewModel / InvoiceViewerViewModel), qui partagent la même expérience
/// visuelle d'édition/aperçu.
/// </summary>
public sealed class ShellViewModel : ViewModelBase
{
    private readonly IServiceProvider _services;
    private readonly DemoDataSeeder _seeder;

    public ShellViewModel(IServiceProvider services, DemoDataSeeder seeder)
    {
        _services = services;
        _seeder = seeder;

        GoToDashboardCommand = new RelayCommand(_ => _ = GoToDashboardAsync());
        GoToQuotesCommand = new RelayCommand(_ => _ = GoToQuotesAsync());
        GoToInvoicesCommand = new RelayCommand(_ => _ = GoToInvoicesAsync());
        GoToPaymentsCommand = new RelayCommand(_ => _ = GoToPaymentsAsync());
    }

    private object? _currentView;
    public object? CurrentView { get => _currentView; private set => SetField(ref _currentView, value); }

    private string _activeSection = "Tableau de bord";
    public string ActiveSection { get => _activeSection; private set => SetField(ref _activeSection, value); }

    private static readonly string[] DailyTips =
    [
        "Tu peux dupliquer un devis en un clic pour gagner du temps !",
        "Un devis accepté peut être transformé en facture en un clic.",
        "Pense à relancer les factures en retard depuis l'onglet Paiements.",
        "Ajoute une remise globale pour les gros chantiers : elle se recalcule en direct.",
    ];

    public string DailyTip { get; } = DailyTips[DateTime.Today.DayOfYear % DailyTips.Length];

    public RelayCommand GoToDashboardCommand { get; }
    public RelayCommand GoToQuotesCommand { get; }
    public RelayCommand GoToInvoicesCommand { get; }
    public RelayCommand GoToPaymentsCommand { get; }

    public async Task InitializeAsync()
    {
        await _seeder.SeedIfEmptyAsync();
        await GoToDashboardAsync();
    }

    public async Task GoToDashboardAsync()
    {
        var vm = _services.GetRequiredService<DashboardViewModel>();
        vm.QuoteSelected += quote => _ = OpenQuoteOrItsInvoiceAsync(quote);
        vm.NewQuoteRequested += () => _ = CreateNewQuoteAsync();
        await vm.InitializeAsync();
        CurrentView = vm;
        ActiveSection = "Tableau de bord";
    }

    /// <summary>Crée un nouveau devis en brouillon pour le premier client trouvé et ouvre l'atelier (démo mono-client).</summary>
    public async Task CreateNewQuoteAsync()
    {
        var runner = _services.GetRequiredService<ScopedUseCaseRunner>();
        var customers = await runner.RunAsync<Application.Customers.SearchCustomersUseCase, IReadOnlyList<CustomerSummaryDto>>(
            uc => uc.ExecuteAsync(null));

        var customer = customers.FirstOrDefault();
        if (customer is null)
            return;

        var vm = _services.GetRequiredService<QuoteEditorViewModel>();
        await vm.CreateNewAsync(customer.Id);
        CurrentView = vm;
        ActiveSection = "Devis";
    }

    public async Task GoToQuotesAsync()
    {
        var vm = _services.GetRequiredService<QuoteListViewModel>();
        vm.QuoteSelected += quote => _ = OpenQuoteOrItsInvoiceAsync(quote);
        await vm.InitializeAsync();
        CurrentView = vm;
        ActiveSection = "Devis";
    }

    /// <summary>
    /// Un devis déjà transformé en facture n'a plus rien à faire dans l'atelier "devis" :
    /// c'est sa facture qui est désormais le document vivant.
    /// </summary>
    private Task OpenQuoteOrItsInvoiceAsync(QuoteSummaryDto quote) =>
        quote.ConvertedInvoiceId is { } invoiceId ? OpenInvoiceAsync(invoiceId) : OpenQuoteAsync(quote.Id);

    public async Task GoToInvoicesAsync()
    {
        var vm = _services.GetRequiredService<InvoiceListViewModel>();
        vm.InvoiceSelected += id => _ = OpenInvoiceAsync(id);
        await vm.InitializeAsync();
        CurrentView = vm;
        ActiveSection = "Factures";
    }

    public async Task GoToPaymentsAsync()
    {
        var vm = _services.GetRequiredService<PaymentListViewModel>();
        vm.InvoiceSelected += id => _ = OpenInvoiceAsync(id);
        await vm.InitializeAsync();
        CurrentView = vm;
        ActiveSection = "Paiements";
    }

    public async Task OpenQuoteAsync(Guid quoteId)
    {
        var vm = _services.GetRequiredService<QuoteEditorViewModel>();
        await vm.LoadAsync(quoteId);
        CurrentView = vm;
        ActiveSection = "Devis";
    }

    public async Task OpenInvoiceAsync(Guid invoiceId)
    {
        var vm = _services.GetRequiredService<InvoiceViewerViewModel>();
        await vm.LoadAsync(invoiceId);
        CurrentView = vm;
        ActiveSection = "Factures";
    }
}
