using SuperDevFact.Domain.Common;
using Xunit;

namespace SuperDevFact.Domain.Tests.Common;

public class MoneyTests
{
    [Theory]
    [InlineData(10.004, 10.00)]
    [InlineData(10.005, 10.01)] // arrondi commercial : la moitié part vers le haut
    [InlineData(10.006, 10.01)]
    [InlineData(-10.005, -10.01)]
    public void Construction_arrondit_au_centime_avec_la_regle_commerciale(decimal input, decimal expected)
    {
        var money = new Money(input);

        Assert.Equal(expected, money.Amount);
    }

    [Fact]
    public void Addition_de_deux_montants_dans_la_meme_devise()
    {
        var result = new Money(10.50m) + new Money(2.25m);

        Assert.Equal(12.75m, result.Amount);
    }

    [Fact]
    public void Addition_de_devises_differentes_leve_une_exception()
    {
        var euros = new Money(10m, "EUR");
        var dollars = new Money(10m, "USD");

        Assert.Throws<InvalidOperationException>(() => euros + dollars);
    }

    [Fact]
    public void Multiplication_par_un_facteur_decimal()
    {
        var result = new Money(100m) * 0.2m;

        Assert.Equal(20m, result.Amount);
    }

    [Fact]
    public void Comparaisons_entre_montants()
    {
        var petit = new Money(10m);
        var grand = new Money(20m);

        Assert.True(grand > petit);
        Assert.True(petit < grand);
        Assert.True(grand >= grand);
        Assert.True(petit <= petit);
    }
}
