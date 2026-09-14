namespace SuperDevFact.Desktop.ViewModels.Dashboard;

/// <summary>
/// Évolution d'un compteur entre les 30 derniers jours et les 30 jours précédents.
/// Calculée à partir des vraies dates des devis/factures — jamais une valeur inventée.
/// </summary>
public sealed record MetricTrend(int Value, decimal PercentChange, string Direction)
{
    public static MetricTrend Compute(int currentPeriod, int previousPeriod, int totalValue)
    {
        if (previousPeriod == 0)
            return new MetricTrend(totalValue, currentPeriod > 0 ? 100m : 0m, currentPeriod > 0 ? "Up" : "Flat");

        var change = Math.Round((currentPeriod - previousPeriod) / (decimal)previousPeriod * 100m, 0);
        var direction = change > 0 ? "Up" : change < 0 ? "Down" : "Flat";
        return new MetricTrend(totalValue, Math.Abs(change), direction);
    }
}
