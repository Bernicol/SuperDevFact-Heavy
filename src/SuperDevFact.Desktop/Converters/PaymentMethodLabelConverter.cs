using System.Globalization;
using System.Windows.Data;

namespace SuperDevFact.Desktop.Converters;

public sealed class PaymentMethodLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture) => value switch
    {
        "BankTransfer" => "Virement",
        "Card" => "Carte bancaire",
        "Check" => "Chèque",
        "Cash" => "Espèces",
        _ => "Autre"
    };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
