using System.Globalization;
using System.Windows.Data;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Vrai si la valeur liée égale le paramètre du convertisseur (utilisé pour piloter l'item actif de la sidebar).</summary>
public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) =>
        string.Equals(value as string, parameter as string, StringComparison.Ordinal);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
