using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Documents;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Application.Quotes;

public sealed class CreateQuoteUseCase(ICustomerRepository customers, IQuoteRepository quotes, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<QuoteDetailsDto> ExecuteAsync(CreateQuoteRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await customers.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException($"Client {request.CustomerId} introuvable.");

        var issueDate = request.IssueDate ?? clock.Today;
        var sequence = await quotes.GetNextSequenceForYearAsync(issueDate.Year, cancellationToken);
        var number = QuoteNumber.Create(issueDate.Year, sequence);
        var paymentTerms = request.PaymentTermsDays is { } days
            ? PaymentTerms.NetDays(days)
            : customer.DefaultPaymentTerms;

        var quote = new Quote(number, customer.Id, issueDate, request.ValidityDays, paymentTerms, request.ClientReference);

        await quotes.AddAsync(quote, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteMapper.ToDto(quote, customer);
    }
}
