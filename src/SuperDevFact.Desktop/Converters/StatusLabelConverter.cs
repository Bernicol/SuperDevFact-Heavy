using System.Globalization;
using System.Windows.Data;

namespace SuperDevFact.Desktop.Converters;

/// <summary>
/// Traduit les statuts techniques du domaine (Draft, Sent, Accepted...) en libellés
/// français affichés à l'utilisateur. Centralisé ici pour que l'écriture du domaine en
/// anglais (conventionnelle en C#) n'ait pas à fuiter dans chaque écran.
/// </summary>
public sealed class StatusLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        "Draft" => "Brouillon",
        "Sent" => "Envoyé",
        "Accepted" => "Accepté",
        "Declined" => "Refusé",
        "Expired" => "Expiré",
        "PartiallyPaid" => "Partiellement payée",
        "Paid" => "Payée",
        "Cancelled" => "Annulée",
        "Overdue" => "En retard",
        "Converted" => "Facturé",
        _ => value?.ToString() ?? string.Empty
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
