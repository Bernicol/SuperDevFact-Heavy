using SuperDevFact.Domain.Common;

namespace SuperDevFact.Domain.Documents;

/// <summary>
/// Calcule les totaux HT / remise / TVA / TTC d'un ensemble de lignes.
/// Règle importante : la remise globale est répercutée proportionnellement sur chaque
/// ligne avant répartition par taux de TVA, puis la TVA est arrondie une fois par taux
/// (et non ligne par ligne) afin d'éviter les écarts d'arrondi cumulés — c'est la pratique
/// comptable standard.
/// </summary>
public static class DocumentTotalsCalculator
{
    public static DocumentTotals Calculate(
        IReadOnlyCollection<DocumentLine> lines,
        Percentage globalDiscountRate,
        string currency = Money.DefaultCurrency)
    {
        var subtotalHt = lines.Aggregate(Money.Zero(currency), static (acc, line) => acc + line.NetAmountHt);
        var globalDiscountAmount = subtotalHt * globalDiscountRate.AsRatio();
        var netHt = subtotalHt - globalDiscountAmount;

        // Ratio du HT net après remise globale par rapport au HT brut, appliqué à chaque
        // ligne pour obtenir sa base taxable réelle.
        var remainingRatio = subtotalHt.Amount == 0m ? 1m : netHt.Amount / subtotalHt.Amount;

        var breakdown = lines
            .GroupBy(line => line.TaxRate)
            .Select(group =>
            {
                var taxableBase = new Money(group.Sum(line => line.NetAmountHt.Amount * remainingRatio), currency);
                var taxAmount = taxableBase * group.Key.Rate.AsRatio();
                return new TaxBreakdownLine(group.Key, taxableBase, taxAmount);
            })
            .OrderByDescending(b => b.TaxRate.Rate.Value)
            .ToList();

        var totalTax = breakdown.Aggregate(Money.Zero(currency), static (acc, b) => acc + b.TaxAmount);
        var totalTtc = netHt + totalTax;

        return new DocumentTotals(subtotalHt, globalDiscountAmount, netHt, breakdown, totalTax, totalTtc);
    }
}
