using SuperDevFact.Domain.Common;

namespace SuperDevFact.Domain.Documents;

/// <summary>
/// Base commune à une ligne de devis ou de facture : une désignation, une quantité,
/// un prix unitaire HT, une remise et un taux de TVA. Portée dans le domaine (et non
/// dupliquée entre devis et facture) car les règles de calcul d'une ligne sont identiques
/// dans les deux cas.
/// </summary>
public abstract class DocumentLine
{
    public Guid Id { get; }
    public int Position { get; private set; }
    public string Description { get; private set; }
    public string? Detail { get; private set; }
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; }
    public Money UnitPriceHt { get; private set; }
    public Percentage DiscountRate { get; private set; }
    public TaxRate TaxRate { get; private set; } = TaxRate.Standard;

    protected DocumentLine(
        string description,
        string? detail,
        decimal quantity,
        string unit,
        Money unitPriceHt,
        Percentage discountRate,
        TaxRate taxRate)
    {
        Id = Guid.NewGuid();
        Description = string.Empty;
        Unit = string.Empty;
        SetContent(description, detail, quantity, unit, unitPriceHt, discountRate, taxRate);
    }

    /// <summary>Montant HT avant remise de ligne (quantité x prix unitaire).</summary>
    public Money GrossAmountHt => UnitPriceHt * Quantity;

    /// <summary>Montant de la remise de ligne.</summary>
    public Money DiscountAmount => GrossAmountHt * DiscountRate.AsRatio();

    /// <summary>Montant HT net, après remise de ligne (c'est le "Total HT" affiché par ligne).</summary>
    public Money NetAmountHt => GrossAmountHt - DiscountAmount;

    public void SetContent(
        string description,
        string? detail,
        decimal quantity,
        string unit,
        Money unitPriceHt,
        Percentage discountRate,
        TaxRate taxRate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("La désignation est obligatoire.", nameof(description));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "La quantité doit être strictement positive.");
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("L'unité est obligatoire.", nameof(unit));
        if (taxRate is null)
            throw new ArgumentNullException(nameof(taxRate));

        Description = description;
        Detail = detail;
        Quantity = quantity;
        Unit = unit;
        UnitPriceHt = unitPriceHt;
        DiscountRate = discountRate;

        // Clone défensif : TaxRate est un value object (record, type référence). Sans ce
        // clone, réutiliser la même instance (ex. TaxRate.Standard, ou recopier le taux
        // d'une ligne de devis vers une ligne de facture) ferait suivre par EF Core le
        // même objet "owned" depuis deux agrégats différents, ce qui casse le suivi de
        // changements. Un Percentage/Money (struct) n'a pas ce problème : il est copié
        // par valeur.
        TaxRate = taxRate with { };
    }

    internal void AssignPosition(int position) => Position = position;
}
