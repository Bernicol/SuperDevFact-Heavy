using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Quotes;
using Xunit;

namespace SuperDevFact.IntegrationTests;

[Collection("Database")]
public sealed class QuotePersistenceTests
{
    public QuotePersistenceTests(DatabaseFixture _) { }

    [Fact]
    public async Task Un_devis_avec_ses_lignes_est_fidelement_relu_apres_sauvegarde()
    {
        await DatabaseFixture.ResetAsync();
        var services = ServiceProviderFactory.Create(new DateOnly(2026, 12, 10));

        var customer = await services.RunAsync<CreateCustomerUseCase, CustomerSummaryDto>(uc => uc.ExecuteAsync(
            new CreateCustomerRequest(
                "Dupont Architecture", "12 rue des Arts", "75011", "Paris", "France",
                "Sophie Martin", "s.martin@dupont-architecture.fr", "01 42 56 78 90", null, null, 30)));

        var quote = await services.RunAsync<CreateQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(
            new CreateQuoteRequest(customer.Id, new DateOnly(2026, 12, 10), 30)));

        await services.RunAsync<AddQuoteLineUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id,
            new QuoteLineRequest("Conseil et accompagnement", "Audit et recommandations", 3m, "jour", 800m, 0m, 20m)));
        await services.RunAsync<AddQuoteLineUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id,
            new QuoteLineRequest("Licence annuelle", "Accès plateforme et support", 5m, "unité", 120m, 10m, 20m)));

        // Relecture dans un nouveau scope (donc un nouveau DbContext) : on vérifie une
        // vraie persistence en base, pas un simple cache mémoire.
        var reloaded = await services.RunAsync<GetQuoteForEditingUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id));

        Assert.Equal("Dupont Architecture", reloaded.Customer.CompanyName);
        Assert.Equal(2, reloaded.Lines.Count);
        // 3 x 800 (sans remise) + 5 x 120 avec 10% de remise = 2400 + 540
        Assert.Equal(2940.00m, reloaded.Totals.SubtotalHt);
        Assert.Equal(3528.00m, reloaded.Totals.TotalTtc);
        Assert.Equal("Draft", reloaded.Status);
    }

    [Fact]
    public async Task La_numerotation_des_devis_s_incremente_sur_l_annee()
    {
        await DatabaseFixture.ResetAsync();
        var services = ServiceProviderFactory.Create(new DateOnly(2026, 1, 5));

        var customer = await services.RunAsync<CreateCustomerUseCase, CustomerSummaryDto>(uc => uc.ExecuteAsync(
            new CreateCustomerRequest(
                "Atelier du Bois", "3 impasse des Chênes", "44000", "Nantes", "France",
                "Jean Bois", "contact@atelierdubois.fr", null, null, null, 30)));

        var first = await services.RunAsync<CreateQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(new CreateQuoteRequest(customer.Id)));
        var second = await services.RunAsync<CreateQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(new CreateQuoteRequest(customer.Id)));

        Assert.Equal("DEV-2026-0001", first.Number);
        Assert.Equal("DEV-2026-0002", second.Number);
    }
}
