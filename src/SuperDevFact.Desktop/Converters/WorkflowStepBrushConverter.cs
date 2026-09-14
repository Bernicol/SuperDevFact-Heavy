using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SuperDevFact.Desktop.Converters;

/// <summary>
/// Couleur d'une étape du fil "Brouillon - Envoyé - Accepté - Facturé - Payé" :
/// pleine si l'étape est atteinte ou dépassée, neutre sinon. Le paramètre du convertisseur
/// porte l'index de l'étape représentée (0 à 4), la valeur liée est l'étape courante.
/// </summary>
public sealed class WorkflowStepBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        var currentStep = value is int i ? i : 0;
        var thisStep = parameter is string s ? int.Parse(s, culture) : 0;

        var resourceKey = currentStep >= thisStep ? "PrimaryBrush" : "NavyMidBrush";
        return System.Windows.Application.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
