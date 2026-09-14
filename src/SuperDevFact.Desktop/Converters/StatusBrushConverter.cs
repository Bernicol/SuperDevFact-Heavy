using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SuperDevFact.Desktop.Converters;

/// <summary>
/// Résout la couleur de fond d'un badge de statut à partir du statut technique du
/// domaine. Va chercher les brushes dans les ressources du Design System (Colors.xaml)
/// plutôt que de dupliquer des couleurs ici.
/// </summary>
public sealed class StatusBrushConverter : IValueConverter
{
    /// <summary>Si vrai, retourne le brush "plein" (badges de la palette de couleurs) au lieu du brush adouci.</summary>
    public bool Solid { get; set; }

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var resourceKey = value switch
        {
            "Accepted" or "Paid" => Solid ? "SuccessBrush" : "SuccessSoftBrush",
            "Sent" => Solid ? "InfoBrush" : "InfoSoftBrush",
            "PartiallyPaid" or "Pending" => Solid ? "WarningBrush" : "WarningSoftBrush",
            "Declined" or "Cancelled" or "Overdue" => Solid ? "DangerBrush" : "DangerSoftBrush",
            "Draft" or "Expired" => Solid ? "NeutralBrush" : "NeutralSoftBrush",
            "Converted" => Solid ? "PrimaryBrush" : "PrimarySoftBrush",
            _ => Solid ? "NeutralBrush" : "NeutralSoftBrush"
        };

        return System.Windows.Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
