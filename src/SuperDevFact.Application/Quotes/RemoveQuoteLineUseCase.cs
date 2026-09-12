using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Application.Quotes;

public sealed class RemoveQuoteLineUseCase(IQuoteRepository quotes, ICustomerRepository customers, IUnitOfWork unitOfWork)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(Guid quoteId, Guid lineId, CancellationToken cancellationToken = default)
    {
        var (quote, customer) = await QuoteLoader.LoadAsync(quotes, customers, quoteId, cancellationToken);

        quote.RemoveLine(lineId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
