using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Customers;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Application.Quotes;

/// <summary>
/// Petite fabrique interne partagée par les cas d'usage d'édition de devis : la plupart
/// doivent charger le devis puis son client pour reconstruire un <see cref="QuoteDetailsDto"/>
/// complet en retour. Évite de dupliquer ces deux appels dans chaque cas d'usage.
/// </summary>
internal static class QuoteLoader
{
    public static async Task<(Quote Quote, Customer Customer)> LoadAsync(
        IQuoteRepository quotes,
        ICustomerRepository customers,
        Guid quoteId,
        CancellationToken cancellationToken)
    {
        var quote = await quotes.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new NotFoundException($"Devis {quoteId} introuvable.");

        var customer = await customers.GetByIdAsync(quote.CustomerId, cancellationToken)
            ?? throw new NotFoundException($"Client {quote.CustomerId} introuvable.");

        return (quote, customer);
    }
}
