using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperDevFact.Application;
using SuperDevFact.Desktop.Infrastructure;
using SuperDevFact.Infrastructure;

namespace SuperDevFact.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Filet de sécurité global : une règle métier violée (ex. statut invalide) ou
        // une coupure réseau/base ne doit jamais faire planter toute l'application —
        // elle doit se traduire par un message compréhensible pour l'utilisateur.
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddJsonFile("appsettings.Secrets.json", optional: true)
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("SUPERDEVFACT_CONNECTION_STRING")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Aucune chaîne de connexion PostgreSQL configurée.");

        var services = new ServiceCollection()
            .AddApplication()
            .AddInfrastructure(connectionString);

        services.AddSingleton<ScopedUseCaseRunner>();
        services.AddSingleton<DemoDataSeeder>();
        services.AddTransient<MainWindow>();
        services.AddTransient<ViewModels.Shell.ShellViewModel>();
        services.AddTransient<ViewModels.Dashboard.DashboardViewModel>();
        services.AddTransient<ViewModels.Quotes.QuoteEditorViewModel>();
        services.AddTransient<ViewModels.Quotes.QuoteListViewModel>();
        services.AddTransient<ViewModels.Invoices.InvoiceListViewModel>();
        services.AddTransient<ViewModels.Invoices.PaymentListViewModel>();
        services.AddTransient<ViewModels.Invoices.InvoiceViewerViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            e.Exception.Message,
            "Action impossible",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        e.Handled = true;
    }
}
