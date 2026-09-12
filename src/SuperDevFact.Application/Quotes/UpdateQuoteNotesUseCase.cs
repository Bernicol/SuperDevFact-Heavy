using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Application.Quotes;

public sealed class UpdateQuoteNotesUseCase(IQuoteRepository quotes, ICustomerRepository customers, IUnitOfWork unitOfWork)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(
        Guid quoteId, string? internalNotes, string? clientMessage, CancellationToken cancellationToken = default)
    {
        var (quote, customer) = await QuoteLoader.LoadAsync(quotes, customers, quoteId, cancellationToken);

        quote.UpdateNotes(internalNotes, clientMessage);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
