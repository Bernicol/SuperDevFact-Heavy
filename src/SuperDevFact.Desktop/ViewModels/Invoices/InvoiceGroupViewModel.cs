using Material.Icons;

namespace SuperDevFact.Desktop.ViewModels.Invoices;

/// <summary>Une étape du pipeline facture (ex. "En retard") avec les factures qu'elle contient.</summary>
public sealed record InvoiceGroupViewModel(string Title, string Hint, MaterialIconKind Icon, string ColorHex, IReadOnlyList<InvoiceListItemViewModel> Items);

/// <summary>Compteur de la ligne de stats en tête de la liste des factures.</summary>
public sealed record InvoiceStatItemViewModel(string Label, MaterialIconKind Icon, string ColorHex, int Count, decimal Total);
