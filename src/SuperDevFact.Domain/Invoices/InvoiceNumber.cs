using System.Text.RegularExpressions;

namespace SuperDevFact.Domain.Invoices;

/// <summary>Numéro de facture au format FAC-AAAA-NNNN (ex. FAC-2026-0082).</summary>
public readonly partial record struct InvoiceNumber
{
    public string Value { get; }

    public InvoiceNumber(string value)
    {
        if (!FormatPattern().IsMatch(value))
            throw new ArgumentException($"Le numéro de facture « {value} » ne respecte pas le format FAC-AAAA-NNNN.", nameof(value));

        Value = value;
    }

    public static InvoiceNumber Create(int year, int sequence) => new($"FAC-{year:D4}-{sequence:D4}");

    public override string ToString() => Value;

    [GeneratedRegex(@"^FAC-\d{4}-\d{4}$")]
    private static partial Regex FormatPattern();
}
