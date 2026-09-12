using SuperDevFact.Domain.Common;
using Xunit;

namespace SuperDevFact.Domain.Tests.Common;

public class PercentageTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void Une_valeur_hors_bornes_leve_une_exception(decimal value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Percentage(value));
    }

    [Fact]
    public void AsRatio_convertit_correctement()
    {
        var percentage = new Percentage(20m);

        Assert.Equal(0.20m, percentage.AsRatio());
    }
}
