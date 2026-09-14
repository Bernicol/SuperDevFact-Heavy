namespace SuperDevFact.Desktop.ViewModels.Quotes;

/// <summary>
/// Ligne de devis dans l'éditeur. Représente l'état "en cours d'édition" en mémoire ;
/// les calculs (total HT de la ligne) sont recalculés côté Domain (même moteur que la
/// persistance) à chaque changement, mais la ligne n'est envoyée en base que lors de
/// l'enregistrement explicite (bouton "Enregistrer").
/// </summary>
public sealed class QuoteLineRowViewModel : ViewModelBase
{
    /// <summary>Null si la ligne n'a jamais été persistée.</summary>
    public Guid? Id { get; set; }

    private int _position;
    public int Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set { if (SetField(ref _description, value)) IsDirty = true; }
    }

    private string? _detail;
    public string? Detail
    {
        get => _detail;
        set { if (SetField(ref _detail, value)) IsDirty = true; }
    }

    private decimal _quantity = 1m;
    public decimal Quantity
    {
        get => _quantity;
        set { if (SetField(ref _quantity, value)) IsDirty = true; }
    }

    private string _unit = "unité";
    public string Unit
    {
        get => _unit;
        set { if (SetField(ref _unit, value)) IsDirty = true; }
    }

    private decimal _unitPriceHt;
    public decimal UnitPriceHt
    {
        get => _unitPriceHt;
        set { if (SetField(ref _unitPriceHt, value)) IsDirty = true; }
    }

    private decimal _discountRatePercent;
    public decimal DiscountRatePercent
    {
        get => _discountRatePercent;
        set { if (SetField(ref _discountRatePercent, value)) IsDirty = true; }
    }

    private decimal _taxRatePercent = 20m;
    public decimal TaxRatePercent
    {
        get => _taxRatePercent;
        set { if (SetField(ref _taxRatePercent, value)) IsDirty = true; }
    }

    private decimal _lineTotalHt;

    /// <summary>Total HT de la ligne (recalculé par le VM parent, jamais saisi directement).</summary>
    public decimal LineTotalHt
    {
        get => _lineTotalHt;
        set => SetField(ref _lineTotalHt, value);
    }

    /// <summary>Vrai si la ligne a été modifiée depuis le dernier enregistrement.</summary>
    public bool IsDirty { get; set; }

    public bool IsNew => Id is null;
}
