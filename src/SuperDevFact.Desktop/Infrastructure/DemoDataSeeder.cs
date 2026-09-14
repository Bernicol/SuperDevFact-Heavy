using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Invoices;
using SuperDevFact.Application.Quotes;
using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Desktop.Infrastructure;

/// <summary>
/// Jeu de données de démonstration : Solaris Installation, artisan spécialisé dans la
/// pose, la réparation et la maintenance de panneaux solaires. Rejoue uniquement les cas
/// d'usage Application (aucun accès direct à EF Core) pour rester réaliste et cohérent
/// avec les règles métier. Ne s'exécute que si la base est vide.
/// </summary>
public sealed class DemoDataSeeder(ScopedUseCaseRunner runner)
{
    public async Task SeedIfEmptyAsync()
    {
        var existing = await runner.RunAsync<SearchCustomersUseCase, IReadOnlyList<CustomerSummaryDto>>(uc => uc.ExecuteAsync(null));
        if (existing.Count > 0)
            return;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var tilleuls = await CreateCustomerAsync("Résidence Les Tilleuls", "12 avenue de la Résidence", "33600", "Pessac", "Marie Lefort", "contact@lestilleuls.fr", "05 56 12 34 56", 30);
        var mairie = await CreateCustomerAsync("Mairie de Mérignac", "1 place Charles de Gaulle", "33700", "Mérignac", "Paul Andrieu", "services.techniques@merignac.fr", "05 56 55 66 77", 45);
        var camping = await CreateCustomerAsync("Camping du Lac Bleu", "8 route du Lac", "33380", "Biscarrosse", "Nadia Costa", "contact@laclacbleu.fr", "05 58 78 90 12", 30);
        var sci = await CreateCustomerAsync("SCI Bellevue", "3 allée Bellevue", "33400", "Talence", "Éric Fontaine", "e.fontaine@sci-bellevue.fr", "05 56 11 22 33", 30);
        var dubois = await CreateCustomerAsync("Martin Dubois", "27 rue des Pins", "33700", "Mérignac", "Martin Dubois", "martin.dubois@mail.fr", "06 12 34 56 78", 30);
        var hotel = await CreateCustomerAsync("Hôtel Le Phare", "2 boulevard de la Plage", "33120", "Arcachon", "Camille Roy", "direction@hotellephare.fr", "05 56 88 99 00", 30);

        // Brouillon
        await CreateQuoteAsync(tilleuls.Id, today,
            ("Installation panneaux solaires", "Pose et raccordement toiture collective", 1m, "jour", 650m, 0m));

        // Envoyé
        var sent = await CreateQuoteAsync(mairie.Id, today.AddDays(-3),
            ("Installation panneaux solaires", "Pose sur bâtiment communal", 4m, "jour", 650m, 5m),
            ("Maintenance annuelle panneaux solaires", "Contrat d'entretien 1ère année", 1m, "forfait", 350m, 0m));
        await runner.RunAsync<SendQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(sent.Id));

        // Accepté, pas encore transformé
        var accepted = await CreateQuoteAsync(camping.Id, today.AddDays(-10),
            ("Réparation panneaux solaires", "Remplacement onduleur défectueux", 3m, "heure", 90m, 0m));
        await runner.RunAsync<SendQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(accepted.Id));
        await runner.RunAsync<AcceptQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(accepted.Id));

        // Accepté puis transformé, facture payée intégralement
        var paidInvoice = await CreateAcceptedAndConvertedAsync(sci.Id, today.AddDays(-40), today.AddDays(-38),
            ("Installation panneaux solaires", "Pose et raccordement", 5m, "jour", 650m, 0m));
        var paidTotal = paidInvoice.Totals.TotalTtc;
        await runner.RunAsync<RecordPaymentUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(
            paidInvoice.Id, new RecordPaymentRequest(paidTotal, today.AddDays(-20), PaymentMethod.BankTransfer, "VIR-2026-014")));

        // Accepté puis transformé, facture partiellement payée
        var partialInvoice = await CreateAcceptedAndConvertedAsync(dubois.Id, today.AddDays(-15), today.AddDays(-13),
            ("Réparation panneaux solaires", "Remplacement panneau fissuré", 2m, "heure", 90m, 0m),
            ("Maintenance annuelle panneaux solaires", "Contrat d'entretien", 1m, "forfait", 350m, 10m));
        await runner.RunAsync<RecordPaymentUseCase, InvoiceDetailsDto>(uc => uc.ExecuteAsync(
            partialInvoice.Id, new RecordPaymentRequest(200m, today.AddDays(-5), PaymentMethod.Card, "CB-2026-041")));

        // Accepté puis transformé, facture en retard (non payée, échéance dépassée)
        await CreateAcceptedAndConvertedAsync(hotel.Id, today.AddDays(-60), today.AddDays(-58),
            ("Installation panneaux solaires", "Pose toiture hôtel", 8m, "jour", 650m, 8m));

        // Devis refusé, pour la variété des statuts
        var declined = await CreateQuoteAsync(tilleuls.Id, today.AddDays(-20),
            ("Maintenance annuelle panneaux solaires", "Option premium", 1m, "forfait", 490m, 0m));
        await runner.RunAsync<SendQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(declined.Id));
        await runner.RunAsync<DeclineQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(declined.Id));
    }

    private Task<CustomerSummaryDto> CreateCustomerAsync(
        string name, string street, string postalCode, string city, string contact, string email, string phone, int paymentTermsDays) =>
        runner.RunAsync<CreateCustomerUseCase, CustomerSummaryDto>(uc => uc.ExecuteAsync(new CreateCustomerRequest(
            name, street, postalCode, city, "France", contact, email, phone, null, null, paymentTermsDays)));

    private async Task<QuoteDetailsDto> CreateQuoteAsync(
        Guid customerId, DateOnly issueDate, params (string Description, string Detail, decimal Quantity, string Unit, decimal UnitPriceHt, decimal DiscountPercent)[] lines)
    {
        var quote = await runner.RunAsync<CreateQuoteUseCase, QuoteDetailsDto>(
            uc => uc.ExecuteAsync(new CreateQuoteRequest(customerId, issueDate)));

        foreach (var line in lines)
        {
            quote = await runner.RunAsync<AddQuoteLineUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(
                quote.Id, new QuoteLineRequest(line.Description, line.Detail, line.Quantity, line.Unit, line.UnitPriceHt, line.DiscountPercent, 20m)));
        }

        return quote;
    }

    private async Task<InvoiceDetailsDto> CreateAcceptedAndConvertedAsync(
        Guid customerId, DateOnly issueDate, DateOnly invoiceIssueDate,
        params (string Description, string Detail, decimal Quantity, string Unit, decimal UnitPriceHt, decimal DiscountPercent)[] lines)
    {
        var quote = await CreateQuoteAsync(customerId, issueDate, lines);
        await runner.RunAsync<SendQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id));
        await runner.RunAsync<AcceptQuoteUseCase, QuoteDetailsDto>(uc => uc.ExecuteAsync(quote.Id));
        return await runner.RunAsync<ConvertQuoteToInvoiceUseCase, InvoiceDetailsDto>(
            uc => uc.ExecuteAsync(quote.Id, invoiceIssueDate));
    }
}
