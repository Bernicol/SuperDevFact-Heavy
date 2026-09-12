using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;
using SuperDevFact.Domain.Quotes;
using Xunit;

namespace SuperDevFact.Domain.Tests.Documents;

public class DocumentTotalsCalculatorTests
{
    [Fact]
    public void Reproduit_l_exemple_de_reference_de_l_editeur_de_devis()
    {
        // Conseil et accompagnement : 3 x 800€, Formation équipe : 1 x 700€,
        // Licence annuelle : 5 x 120€ avec 10% de remise.
        var lines = new DocumentLine[]
        {
            new QuoteLine("Conseil et accompagnement", null, 3m, "jour", new Money(800m), Percentage.Zero, TaxRate.Standard),
            new QuoteLine("Formation équipe", null, 1m, "jour", new Money(700m), Percentage.Zero, TaxRate.Standard),
            new QuoteLine("Licence annuelle", null, 5m, "unité", new Money(120m), new Percentage(10m), TaxRate.Standard),
        };

        var totals = DocumentTotalsCalculator.Calculate(lines, Percentage.Zero);

        Assert.Equal(3640.00m, totals.SubtotalHt.Amount);
        Assert.Equal(0m, totals.GlobalDiscountAmount.Amount);
        Assert.Equal(3640.00m, totals.NetHt.Amount);
        Assert.Equal(728.00m, totals.TotalTax.Amount);
        Assert.Equal(4368.00m, totals.TotalTtc.Amount);
    }

    [Fact]
    public void Une_remise_globale_est_repercutee_avant_calcul_de_la_TVA()
    {
        var lines = new DocumentLine[]
        {
            new QuoteLine("Prestation", null, 1m, "forfait", new Money(1000m), Percentage.Zero, TaxRate.Standard),
        };

        var totals = DocumentTotalsCalculator.Calculate(lines, new Percentage(10m));

        Assert.Equal(1000.00m, totals.SubtotalHt.Amount);
        Assert.Equal(100.00m, totals.GlobalDiscountAmount.Amount);
        Assert.Equal(900.00m, totals.NetHt.Amount);
        Assert.Equal(180.00m, totals.TotalTax.Amount); // 20% de 900
        Assert.Equal(1080.00m, totals.TotalTtc.Amount);
    }

    [Fact]
    public void Les_lignes_a_taux_de_TVA_differents_sont_ventilees_separement()
    {
        var lines = new DocumentLine[]
        {
            new QuoteLine("Prestation taux normal", null, 1m, "forfait", new Money(100m), Percentage.Zero, TaxRate.Standard),
            new QuoteLine("Livre (taux réduit)", null, 1m, "unité", new Money(100m), Percentage.Zero, TaxRate.Reduced),
        };

        var totals = DocumentTotalsCalculator.Calculate(lines, Percentage.Zero);

        Assert.Equal(2, totals.TaxBreakdown.Count);
        Assert.Contains(totals.TaxBreakdown, b => b.TaxRate == TaxRate.Standard && b.TaxAmount.Amount == 20.00m);
        Assert.Contains(totals.TaxBreakdown, b => b.TaxRate == TaxRate.Reduced && b.TaxAmount.Amount == 5.50m);
        Assert.Equal(225.50m, totals.TotalTtc.Amount);
    }

    [Fact]
    public void Un_document_sans_ligne_donne_des_totaux_nuls()
    {
        var totals = DocumentTotalsCalculator.Calculate(Array.Empty<DocumentLine>(), Percentage.Zero);

        Assert.Equal(0m, totals.SubtotalHt.Amount);
        Assert.Equal(0m, totals.TotalTtc.Amount);
        Assert.Empty(totals.TaxBreakdown);
    }
}
