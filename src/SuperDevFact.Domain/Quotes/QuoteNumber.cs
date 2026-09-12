using System.Text.RegularExpressions;

namespace SuperDevFact.Domain.Quotes;

/// <summary>Numéro de devis au format DEV-AAAA-NNNN (ex. DEV-2026-0042).</summary>
public readonly partial record struct QuoteNumber
{
    public string Value { get; }

    public QuoteNumber(string value)
    {
        if (!FormatPattern().IsMatch(value))
            throw new ArgumentException($"Le numéro de devis « {value} » ne respecte pas le format DEV-AAAA-NNNN.", nameof(value));

        Value = value;
    }

    public static QuoteNumber Create(int year, int sequence) => new($"DEV-{year:D4}-{sequence:D4}");

    public override string ToString() => Value;

    [GeneratedRegex(@"^DEV-\d{4}-\d{4}$")]
    private static partial Regex FormatPattern();
}
