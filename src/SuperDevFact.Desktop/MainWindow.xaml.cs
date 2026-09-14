using System.Windows;
using System.Windows.Shell;
using SuperDevFact.Desktop.ViewModels.Shell;

namespace SuperDevFact.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ShellViewModel _shell;

    public MainWindow(ShellViewModel shell)
    {
        InitializeComponent();

        _shell = shell;
        DataContext = _shell;

        Loaded += async (_, _) => await InitializeAsync();

        StateChanged += (_, _) =>
        {
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
            // Avec WindowStyle="None", l'état maximisé déborde par-dessus la barre des
            // tâches et les bords d'écran ; correctif standard WindowChrome + ResizeMode.
            RootBorder.Margin = WindowState == WindowState.Maximized ? new Thickness(7) : new Thickness(0);
        };
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

    private void OnMaximizeClick(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void OnCloseClick(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);

    private async Task InitializeAsync()
    {
        try
        {
            await _shell.InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Impossible de charger les données depuis PostgreSQL :\n{ex.Message}",
                "Erreur de connexion",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
