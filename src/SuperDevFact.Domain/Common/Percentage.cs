namespace SuperDevFact.Domain.Common;

/// <summary>
/// Représente un pourcentage borné entre 0 et 100, utilisé pour les remises et les taux de TVA.
/// </summary>
public readonly record struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0m || value > 100m)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Un pourcentage doit être compris entre 0 et 100.");

        Value = value;
    }

    public static Percentage Zero => new(0m);

    /// <summary>Convertit le pourcentage en ratio décimal (ex. 20% -> 0.20).</summary>
    public decimal AsRatio() => Value / 100m;

    public static implicit operator Percentage(decimal value) => new(value);

    public override string ToString() => $"{Value:0.##}%";
}
