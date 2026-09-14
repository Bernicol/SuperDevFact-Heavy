using System.Globalization;
using System.Windows.Data;
using Material.Icons;

namespace SuperDevFact.Desktop.Converters;

/// <summary>Icône représentant le statut d'un devis ou d'une facture dans les listes.</summary>
public sealed class StatusIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        "Draft" => MaterialIconKind.FileDocumentOutline,
        "Sent" => MaterialIconKind.SendOutline,
        "Accepted" => MaterialIconKind.CheckCircleOutline,
        "Declined" => MaterialIconKind.CloseCircleOutline,
        "Expired" => MaterialIconKind.ClockAlertOutline,
        "PartiallyPaid" => MaterialIconKind.CashClock,
        "Paid" => MaterialIconKind.CashCheck,
        "Cancelled" => MaterialIconKind.CancelOutline,
        "Overdue" => MaterialIconKind.AlertCircleOutline,
        "Converted" => MaterialIconKind.ReceiptTextOutline,
        _ => MaterialIconKind.FileDocumentOutline
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
