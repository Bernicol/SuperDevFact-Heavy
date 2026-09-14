using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Couleur associée à une direction de tendance ("Up"/"Down"/"Flat").</summary>
public sealed class TrendDirectionBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var resourceKey = value switch
        {
            "Up" => "SuccessBrush",
            "Down" => "DangerBrush",
            _ => "NeutralBrush"
        };

        return System.Windows.Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
