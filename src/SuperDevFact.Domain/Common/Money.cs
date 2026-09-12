namespace SuperDevFact.Domain.Common;

/// <summary>
/// Représente un montant monétaire. Toujours arrondi au centime (règle commerciale :
/// arrondi au plus proche, la moitié partant vers le haut) afin d'éviter toute dérive
/// liée à une représentation flottante approximative.
/// </summary>
public readonly record struct Money
{
    public const string DefaultCurrency = "EUR";

    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = DefaultCurrency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("La devise est obligatoire.", nameof(currency));

        Currency = currency;
        Amount = Round(amount);
    }

    public static Money Zero(string currency = DefaultCurrency) => new(0m, currency);

    public static Money operator +(Money left, Money right) => Combine(left, right, static (a, b) => a + b);

    public static Money operator -(Money left, Money right) => Combine(left, right, static (a, b) => a - b);

    public static Money operator *(Money money, decimal factor) => new(money.Amount * factor, money.Currency);

    public static Money operator *(decimal factor, Money money) => money * factor;

    public static bool operator >(Money left, Money right) => Compare(left, right) > 0;

    public static bool operator <(Money left, Money right) => Compare(left, right) < 0;

    public static bool operator >=(Money left, Money right) => Compare(left, right) >= 0;

    public static bool operator <=(Money left, Money right) => Compare(left, right) <= 0;

    private static Money Combine(Money left, Money right, Func<decimal, decimal, decimal> operation)
    {
        EnsureSameCurrency(left, right);
        return new Money(operation(left.Amount, right.Amount), left.Currency);
    }

    private static int Compare(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return left.Amount.CompareTo(right.Amount);
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new InvalidOperationException(
                $"Impossible d'opérer sur des montants de devises différentes ({left.Currency} et {right.Currency}).");
        }
    }

    private static decimal Round(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    public override string ToString() => $"{Amount:N2} {Currency}";
}
