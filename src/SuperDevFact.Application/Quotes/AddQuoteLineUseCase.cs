using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Common;
using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Application.Quotes;

public sealed class AddQuoteLineUseCase(IQuoteRepository quotes, ICustomerRepository customers, IUnitOfWork unitOfWork)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(Guid quoteId, QuoteLineRequest request, CancellationToken cancellationToken = default)
    {
        var (quote, customer) = await QuoteLoader.LoadAsync(quotes, customers, quoteId, cancellationToken);

        quote.AddLine(
            request.Description,
            request.Detail,
            request.Quantity,
            request.Unit,
            new Money(request.UnitPriceHt),
            new Percentage(request.DiscountRatePercent),
            TaxRate.FromPercent(request.TaxRatePercent));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
