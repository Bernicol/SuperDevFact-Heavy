using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;
using SuperDevFact.Domain.Quotes;
using Xunit;

namespace SuperDevFact.Domain.Tests.Quotes;

public class QuoteTests
{
    private static Quote CreateDraftQuoteWithOneLine()
    {
        var quote = new Quote(
            QuoteNumber.Create(2026, 42),
            Guid.NewGuid(),
            new DateOnly(2026, 12, 10),
            30,
            PaymentTerms.NetDays(30));

        quote.AddLine("Conseil et accompagnement", "Audit et recommandations", 3m, "jour", new Money(800m), Percentage.Zero, TaxRate.Standard);
        return quote;
    }

    [Fact]
    public void Un_devis_nait_en_brouillon()
    {
        var quote = CreateDraftQuoteWithOneLine();

        Assert.Equal(QuoteStatus.Draft, quote.Status);
    }

    [Fact]
    public void On_ne_peut_pas_envoyer_un_devis_sans_ligne()
    {
        var quote = new Quote(QuoteNumber.Create(2026, 1), Guid.NewGuid(), new DateOnly(2026, 1, 1), 30, PaymentTerms.Immediate);

        Assert.Throws<InvalidOperationException>(() => quote.Send());
    }

    [Fact]
    public void Le_cycle_de_vie_nominal_Draft_Sent_Accepted_est_respecte()
    {
        var quote = CreateDraftQuoteWithOneLine();

        quote.Send();
        Assert.Equal(QuoteStatus.Sent, quote.Status);

        quote.Accept();
        Assert.Equal(QuoteStatus.Accepted, quote.Status);
    }

    [Fact]
    public void On_ne_peut_pas_accepter_un_devis_encore_en_brouillon()
    {
        var quote = CreateDraftQuoteWithOneLine();

        Assert.Throws<InvalidOperationException>(() => quote.Accept());
    }

    [Fact]
    public void On_ne_peut_pas_modifier_les_lignes_d_un_devis_envoye()
    {
        var quote = CreateDraftQuoteWithOneLine();
        quote.Send();

        Assert.Throws<InvalidOperationException>(() =>
            quote.AddLine("Ligne tardive", null, 1m, "unité", new Money(50m), Percentage.Zero, TaxRate.Standard));
    }

    [Fact]
    public void Un_devis_refuse_ne_peut_pas_etre_transforme_en_facture()
    {
        var quote = CreateDraftQuoteWithOneLine();
        quote.Send();
        quote.Decline();

        Assert.Equal(QuoteStatus.Declined, quote.Status);
        Assert.Throws<InvalidOperationException>(() => quote.MarkConverted(Guid.NewGuid()));
    }

    [Fact]
    public void Un_devis_accepte_peut_etre_marque_comme_transforme_une_seule_fois()
    {
        var quote = CreateDraftQuoteWithOneLine();
        quote.Send();
        quote.Accept();

        quote.MarkConverted(Guid.NewGuid());

        Assert.NotNull(quote.ConvertedInvoiceId);
        Assert.Throws<InvalidOperationException>(() => quote.MarkConverted(Guid.NewGuid()));
    }
}
