using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Invoices;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Infrastructure.Persistence.Conversions;

/// <summary>
/// Convertisseurs EF Core réutilisés pour mapper les value objects du domaine vers des
/// colonnes primitives. Centralisés ici pour éviter de les redéfinir dans chaque
/// configuration d'entité.
/// </summary>
internal static class Converters
{
    // Prototype mono-devise : on ne persiste que le montant, la devise est toujours EUR
    // (Money.DefaultCurrency). Passer au multi-devise nécessiterait de stocker la devise
    // en plus, sans changer le domaine.
    public static readonly ValueConverter<Money, decimal> MoneyConverter =
        new(m => m.Amount, v => new Money(v, Money.DefaultCurrency));

    public static readonly ValueConverter<Percentage, decimal> PercentageConverter =
        new(p => p.Value, v => new Percentage(v));

    public static readonly ValueConverter<QuoteNumber, string> QuoteNumberConverter =
        new(n => n.Value, v => new QuoteNumber(v));

    public static readonly ValueConverter<InvoiceNumber, string> InvoiceNumberConverter =
        new(n => n.Value, v => new InvoiceNumber(v));
}
