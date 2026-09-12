using SuperDevFact.Domain.Common;

namespace SuperDevFact.Domain.Documents;

/// <summary>Détail de la TVA due pour un taux donné.</summary>
public sealed record TaxBreakdownLine(TaxRate TaxRate, Money TaxableBase, Money TaxAmount);

/// <summary>
/// Résultat, entièrement déterministe, du calcul des totaux d'un devis ou d'une facture.
/// </summary>
public sealed record DocumentTotals(
    Money SubtotalHt,
    Money GlobalDiscountAmount,
    Money NetHt,
    IReadOnlyList<TaxBreakdownLine> TaxBreakdown,
    Money TotalTax,
    Money TotalTtc);
