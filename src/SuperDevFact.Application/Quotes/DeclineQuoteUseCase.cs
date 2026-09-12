using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Application.Quotes;

public sealed class DeclineQuoteUseCase(IQuoteRepository quotes, ICustomerRepository customers, IUnitOfWork unitOfWork)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(Guid quoteId, CancellationToken cancellationToken = default)
    {
        var (quote, customer) = await QuoteLoader.LoadAsync(quotes, customers, quoteId, cancellationToken);

        quote.Decline();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
