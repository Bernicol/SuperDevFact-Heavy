using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Application.Quotes;

/// <summary>Charge un devis complet (avec son client) pour l'écran d'édition.</summary>
public sealed class GetQuoteForEditingUseCase(IQuoteRepository quotes, ICustomerRepository customers)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var (quote, customer) = await QuoteLoader.LoadAsync(quotes, customers, quoteId, cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
