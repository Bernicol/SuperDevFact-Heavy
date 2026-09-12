using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;
using SuperDevFact.Domain.Invoices;
using SuperDevFact.Domain.Quotes;
using Xunit;

namespace SuperDevFact.Domain.Tests.Invoices;

public class InvoiceTests
{
    private static Quote CreateAcceptedQuote()
    {
        var quote = new Quote(
            QuoteNumber.Create(2026, 42),
            Guid.NewGuid(),
            new DateOnly(2026, 12, 10),
            30,
            PaymentTerms.NetDays(30));

        quote.AddLine("Conseil et accompagnement", null, 3m, "jour", new Money(800m), Percentage.Zero, TaxRate.Standard);
        quote.AddLine("Licence annuelle", null, 5m, "unité", new Money(120m), new Percentage(10m), TaxRate.Standard);
        quote.Send();
        quote.Accept();
        return quote;
    }

    [Fact]
    public void Convertir_un_devis_non_accepte_est_impossible()
    {
        var quote = new Quote(QuoteNumber.Create(2026, 1), Guid.NewGuid(), new DateOnly(2026, 1, 1), 30, PaymentTerms.Immediate);
        quote.AddLine("Ligne", null, 1m, "unité", new Money(100m), Percentage.Zero, TaxRate.Standard);

        Assert.Throws<InvalidOperationException>(() =>
            Invoice.CreateFromQuote(quote, InvoiceNumber.Create(2026, 1), new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void La_facture_issue_d_un_devis_reprend_fidelement_les_lignes_et_totaux()
    {
        var quote = CreateAcceptedQuote();

        var invoice = Invoice.CreateFromQuote(quote, InvoiceNumber.Create(2026, 82), new DateOnly(2026, 12, 12));

        Assert.Equal(quote.CustomerId, invoice.CustomerId);
        Assert.Equal(quote.Id, invoice.SourceQuoteId);
        Assert.Equal(quote.Lines.Count, invoice.Lines.Count);
        Assert.Equal(quote.CalculateTotals().TotalTtc, invoice.CalculateTotals().TotalTtc);
        Assert.Equal(InvoiceStatus.Sent, invoice.Status);
    }

    [Fact]
    public void L_echeance_est_calculee_a_partir_des_conditions_de_paiement()
    {
        var quote = CreateAcceptedQuote();
        var invoice = Invoice.CreateFromQuote(quote, InvoiceNumber.Create(2026, 82), new DateOnly(2026, 12, 12));

        Assert.Equal(new DateOnly(2027, 1, 11), invoice.DueDate); // +30 jours
    }

    [Fact]
    public void Un_paiement_partiel_place_la_facture_en_attente_du_solde()
    {
        var quote = CreateAcceptedQuote();
        var invoice = Invoice.CreateFromQuote(quote, InvoiceNumber.Create(2026, 82), new DateOnly(2026, 12, 12));

        invoice.RecordPayment(new Money(1000m), new DateOnly(2026, 12, 20), PaymentMethod.BankTransfer);

        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.True(invoice.Balance().Amount > 0m);
    }

    [Fact]
    public void Un_paiement_qui_solde_la_facture_la_marque_comme_payee()
    {
        var quote = CreateAcceptedQuote();
        var invoice = Invoice.CreateFromQuote(quote, InvoiceNumber.Create(2026, 82), new DateOnly(2026, 12, 12));
        var totalDue = invoice.CalculateTotals().TotalTtc;

        invoice.RecordPayment(totalDue, new DateOnly(2026, 12, 20), PaymentMethod.BankTransfer);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(0m, invoice.Balance().Amount);
    }

    [Fact]
    public void On_ne_peut_pas_payer_une_facture_annulee()
    {
        var quote = CreateAcceptedQuote();
        var invoice = Invoice.CreateFromQuote(quote, InvoiceNumber.Create(2026, 82), new DateOnly(2026, 12, 12));
        invoice.Cancel();

        Assert.Throws<InvalidOperationException>(() =>
            invoice.RecordPayment(new Money(100m), new DateOnly(2026, 12, 20), PaymentMethod.Cash));
    }

    [Fact]
    public void Une_facture_est_en_retard_si_la_date_du_jour_depasse_l_echeance_et_qu_elle_n_est_pas_payee()
    {
        var quote = CreateAcceptedQuote();
        var invoice = Invoice.CreateFromQuote(quote, InvoiceNumber.Create(2026, 82), new DateOnly(2026, 12, 12));

        Assert.False(invoice.IsOverdue(new DateOnly(2026, 12, 15)));
        Assert.True(invoice.IsOverdue(new DateOnly(2027, 2, 1)));
    }
}
