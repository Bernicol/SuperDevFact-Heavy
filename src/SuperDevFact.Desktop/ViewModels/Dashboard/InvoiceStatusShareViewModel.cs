namespace SuperDevFact.Desktop.ViewModels.Dashboard;

/// <summary>Ligne du petit récapitulatif affiché à côté du donut "Factures par statut".</summary>
public sealed record InvoiceStatusShareViewModel(string Label, string ColorHex, int Count, decimal PercentShare);
