using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Couleur de texte assortie à <see cref="StatusBrushConverter"/> (le badge utilise un fond adouci, le texte reste dans la teinte pleine).</summary>
public sealed class StatusForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var resourceKey = value switch
        {
            "Accepted" or "Paid" => "SuccessBrush",
            "Sent" => "InfoBrush",
            "PartiallyPaid" or "Pending" => "WarningBrush",
            "Declined" or "Cancelled" or "Overdue" => "DangerBrush",
            "Draft" or "Expired" => "NeutralBrush",
            "Converted" => "PrimaryBrush",
            _ => "NeutralBrush"
        };

        return System.Windows.Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
