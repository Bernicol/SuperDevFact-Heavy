using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Common;

namespace SuperDevFact.Application.Quotes;

public sealed class ApplyGlobalDiscountUseCase(IQuoteRepository quotes, ICustomerRepository customers, IUnitOfWork unitOfWork)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(Guid quoteId, decimal discountRatePercent, CancellationToken cancellationToken = default)
    {
        var (quote, customer) = await QuoteLoader.LoadAsync(quotes, customers, quoteId, cancellationToken);

        quote.ApplyGlobalDiscount(new Percentage(discountRatePercent));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
