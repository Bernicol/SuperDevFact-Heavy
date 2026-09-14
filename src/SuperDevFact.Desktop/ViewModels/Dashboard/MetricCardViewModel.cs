using Material.Icons;

namespace SuperDevFact.Desktop.ViewModels.Dashboard;

/// <summary>Une carte-compteur du tableau de bord (icône, couleur, libellé, évolution réelle).</summary>
public sealed record MetricCardViewModel(string Label, MaterialIconKind Icon, string ColorHex, MetricTrend Trend);
