using Microsoft.Extensions.DependencyInjection;
using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Application.Quotes;
using Xunit;

namespace SuperDevFact.IntegrationTests;

[Collection("Database")]
public sealed class ConvertQuoteToInvoiceIntegrationTests
{
    public ConvertQuoteToInvoiceIntegrationTests(DatabaseFixture _) { }

    private static async Task<(ServiceProvider Services, Guid QuoteId)> CreateAcceptedQuoteAsync()
    {
        var services = ServiceProviderFactory.Create(new DateOnly(2026, 12, 10));

        var customer = await services.RunAsync<CreateCustomerUseCase, CustomerSummaryDto>(uc => uc.ExecuteAsync(
            new CreateCustomerRequest(
                "Martin & Fils", "8 avenue de la République", "69003", "Lyon", "France",
                "Paul Martin", "p.martin@martinetfils.fr", null, null, null, 30)));

        var quote = await services.RunAsync<CreateQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(new CreateQuoteRequest(customer.Id)));

        await services.RunAsync<AddQuoteLineUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id,
            new QuoteLineRequest("Prestation de conseil", null, 2m, "jour", 950m, 0m, 20m)));

        await services.RunAsync<SendQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id));
        await services.RunAsync<AcceptQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id));

        return (services, quote.Id);
    }

    [Fact]
    public async Task Convertir_un_devis_accepte_cree_une_facture_persistee_avec_les_memes_lignes()
    {
        await DatabaseFixture.ResetAsync();
        var (services, quoteId) = await CreateAcceptedQuoteAsync();
        await using var _ = services;

        var invoice = await services.RunAsync<ConvertQuoteToInvoiceUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(quoteId));

        Assert.Equal("FAC-2026-0001", invoice.Number);
        Assert.Single(invoice.Lines);
        Assert.Equal("Sent", invoice.Status);

        var reloadedQuote = await services.RunAsync<GetQuoteForEditingUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quoteId));
        var reloadedInvoice = await services.RunAsync<GetInvoiceForViewingUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(invoice.Id));

        // Les deux écritures (devis marqué converti + facture créée) doivent avoir été
        // persistées ensemble, dans la même transaction.
        Assert.Equal(invoice.Id, reloadedQuote.ConvertedInvoiceId);
        Assert.Equal(quoteId, reloadedInvoice.SourceQuoteId);
        Assert.Equal(reloadedQuote.Totals.TotalTtc, reloadedInvoice.Totals.TotalTtc);
    }

    [Fact]
    public async Task On_ne_peut_pas_transformer_deux_fois_le_meme_devis()
    {
        await DatabaseFixture.ResetAsync();
        var (services, quoteId) = await CreateAcceptedQuoteAsync();
        await using var _ = services;

        await services.RunAsync<ConvertQuoteToInvoiceUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(quoteId));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            services.RunAsync<ConvertQuoteToInvoiceUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(quoteId)));
    }
}
