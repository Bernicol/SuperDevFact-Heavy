using System.Windows.Controls;

namespace SuperDevFact.Desktop.Views.Quotes;

/// <summary>
/// Vue de l'éditeur de devis. Volontairement sans logique métier : toute la logique
/// vit dans <see cref="ViewModels.Quotes.QuoteEditorViewModel"/>.
/// </summary>
public partial class QuoteEditorView : UserControl
{
    public QuoteEditorView()
    {
        InitializeComponent();
    }
}
