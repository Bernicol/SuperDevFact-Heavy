using Material.Icons;

namespace SuperDevFact.Desktop.ViewModels.Dashboard;

/// <summary>
/// Ligne du widget "TaskPulse" : une simulation décorative de gestion de rappels
/// personnels, purement visuelle (aucune donnée réelle, aucune persistance).
/// </summary>
public sealed record TaskPulseItemViewModel(string Text, MaterialIconKind Icon, string ColorHex);
