using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Variante texte de <see cref="WorkflowStepBrushConverter"/> (libellé sous chaque étape).</summary>
public sealed class WorkflowStepTextBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var currentStep = value is int i ? i : 0;
        var thisStep = parameter is string s ? int.Parse(s, culture) : 0;

        var resourceKey = currentStep >= thisStep ? "TextInverseBrush" : "TextMutedBrush";
        return System.Windows.Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
