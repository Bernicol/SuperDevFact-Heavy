using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Visible si la chaîne liée n'est pas vide (utilisé pour les bannières de confirmation).</summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
