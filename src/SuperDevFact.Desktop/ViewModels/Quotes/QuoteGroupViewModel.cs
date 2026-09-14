using Material.Icons;
using SuperDevFact.Application.Quotes;

namespace SuperDevFact.Desktop.ViewModels.Quotes;

/// <summary>Une étape du pipeline devis (ex. "Envoyés") avec les devis qu'elle contient.</summary>
public sealed record QuoteGroupViewModel(string Title, string Hint, MaterialIconKind Icon, string ColorHex, IReadOnlyList<QuoteSummaryDto> Items);

/// <summary>Compteur de la ligne de stats en tête de la liste des devis.</summary>
public sealed record QuoteStatItemViewModel(string Label, MaterialIconKind Icon, string ColorHex, int Count, decimal Total);
