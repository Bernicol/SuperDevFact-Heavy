using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Application.Quotes;

public sealed class UpdateQuoteGeneralInfoUseCase(IQuoteRepository quotes, ICustomerRepository customers, IUnitOfWork unitOfWork)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(
        Guid quoteId, UpdateQuoteGeneralInfoRequest request, CancellationToken cancellationToken = default)
    {
        var quote = await quotes.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new NotFoundException($"Devis {quoteId} introuvable.");

        var customer = await customers.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException($"Client {request.CustomerId} introuvable.");

        quote.UpdateGeneralInformation(
            request.CustomerId,
            request.IssueDate,
            request.ValidityDays,
            PaymentTerms.NetDays(request.PaymentTermsDays),
            request.ClientReference);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
