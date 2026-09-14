using System.Globalization;
using System.Windows.Data;
using Material.Icons;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Icône associée à une direction de tendance ("Up"/"Down"/"Flat").</summary>
public sealed class TrendDirectionIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        "Up" => MaterialIconKind.ArrowTopRight,
        "Down" => MaterialIconKind.ArrowBottomRight,
        _ => MaterialIconKind.ArrowRight
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
