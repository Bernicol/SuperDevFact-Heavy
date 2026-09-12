namespace SuperDevFact.Domain.Documents;

public enum PaymentTermsKind
{
    Immediate,
    NetDays,
    EndOfMonth
}

/// <summary>
/// Conditions de paiement attachées à un devis ou une facture. Sait calculer une date
/// d'échéance à partir d'une date d'émission, ce qui centralise une règle métier simple
/// mais facilement source d'erreurs si dupliquée à plusieurs endroits.
///
/// Comme <see cref="TaxRate"/>, c'est un record (type référence) : <see cref="Immediate"/>
/// est une propriété qui construit une nouvelle instance à chaque accès, pour ne jamais
/// partager le même objet "owned" entre deux agrégats (ex. le client et l'un de ses devis).
/// </summary>
public sealed record PaymentTerms(PaymentTermsKind Kind, int Days, string Label)
{
    public static PaymentTerms Immediate => new(PaymentTermsKind.Immediate, 0, "Comptant");

    public static PaymentTerms NetDays(int days) => new(PaymentTermsKind.NetDays, days, $"{days} jours net");

    public static PaymentTerms EndOfMonth(int days) => new(PaymentTermsKind.EndOfMonth, days, $"{days} jours fin de mois");

    public DateOnly ComputeDueDate(DateOnly issueDate) => Kind switch
    {
        PaymentTermsKind.Immediate => issueDate,
        PaymentTermsKind.NetDays => issueDate.AddDays(Days),
        PaymentTermsKind.EndOfMonth => LastDayOfMonth(issueDate).AddDays(Days),
        _ => issueDate
    };

    private static DateOnly LastDayOfMonth(DateOnly date) =>
        new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);

    public override string ToString() => Label;
}
