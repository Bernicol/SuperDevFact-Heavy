using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Convertit une couleur hexadécimale ("#2F6FED") en Brush, pour les couleurs calculées dynamiquement (légendes de graphiques).</summary>
public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex && !string.IsNullOrWhiteSpace(hex))
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch (FormatException)
            {
                // valeur hexadécimale invalide : on retombe sur le brush neutre ci-dessous
            }
        }

        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
