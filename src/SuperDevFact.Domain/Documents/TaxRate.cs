using SuperDevFact.Domain.Common;

namespace SuperDevFact.Domain.Documents;

/// <summary>
/// Taux de TVA applicable à une ligne de document. Les taux français usuels sont exposés
/// comme instances prédéfinies, mais un taux personnalisé reste possible.
///
/// Important : chaque accès à <see cref="Standard"/> (et consorts) et chaque appel à
/// <see cref="FromPercent"/> construit une nouvelle instance. Ce type est un record
/// (type référence) persisté par EF Core comme "owned type" ; partager la même instance
/// entre plusieurs lignes ou plusieurs agrégats casserait le suivi de changements.
/// </summary>
public sealed record TaxRate(string Label, Percentage Rate)
{
    public static TaxRate Standard => new("Taux normal", new Percentage(20m));
    public static TaxRate Intermediate => new("Taux intermédiaire", new Percentage(10m));
    public static TaxRate Reduced => new("Taux réduit", new Percentage(5.5m));
    public static TaxRate Exempt => new("Exonéré", Percentage.Zero);

    /// <summary>
    /// Retrouve un taux connu à partir de sa valeur (ex. 20 -> Standard), ou construit
    /// un taux personnalisé si aucun taux connu ne correspond exactement.
    /// </summary>
    public static TaxRate FromPercent(decimal percent) => percent switch
    {
        20m => Standard,
        10m => Intermediate,
        5.5m => Reduced,
        0m => Exempt,
        _ => new TaxRate("Taux personnalisé", new Percentage(percent))
    };

    public override string ToString() => $"{Label} ({Rate})";
}
