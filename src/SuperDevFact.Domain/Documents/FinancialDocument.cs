using SuperDevFact.Domain.Common;

namespace SuperDevFact.Domain.Documents;

/// <summary>
/// Base commune à un devis et une facture : gestion des lignes (ajout, retrait,
/// réordonnancement) et calcul des totaux. Factorisée ici car un devis et une facture
/// partagent exactement les mêmes règles de composition et de calcul ; seul le cycle
/// de vie (statuts) diffère et reste défini dans chaque agrégat concret.
/// </summary>
public abstract class FinancialDocument<TLine> where TLine : DocumentLine
{
    private readonly List<TLine> _lines = new();

    public IReadOnlyList<TLine> Lines => _lines;

    public Percentage GlobalDiscountRate { get; protected set; } = Percentage.Zero;

    protected void RegisterLine(TLine line)
    {
        line.AssignPosition(_lines.Count);
        _lines.Add(line);
    }

    protected TLine GetLineOrThrow(Guid lineId) =>
        _lines.FirstOrDefault(l => l.Id == lineId)
        ?? throw new InvalidOperationException("Cette ligne n'existe pas sur ce document.");

    protected void RemoveLineInternal(Guid lineId)
    {
        var line = GetLineOrThrow(lineId);
        _lines.Remove(line);
        Reindex();
    }

    protected void ReorderLinesInternal(IReadOnlyList<Guid> orderedLineIds)
    {
        if (orderedLineIds.Count != _lines.Count)
            throw new ArgumentException("L'ordre fourni doit contenir exactement toutes les lignes du document.", nameof(orderedLineIds));

        var reordered = orderedLineIds.Select(GetLineOrThrow).ToList();
        _lines.Clear();
        _lines.AddRange(reordered);
        Reindex();
    }

    private void Reindex()
    {
        for (var i = 0; i < _lines.Count; i++)
            _lines[i].AssignPosition(i);
    }

    public DocumentTotals CalculateTotals(string currency = Money.DefaultCurrency) =>
        DocumentTotalsCalculator.Calculate(_lines, GlobalDiscountRate, currency);
}
