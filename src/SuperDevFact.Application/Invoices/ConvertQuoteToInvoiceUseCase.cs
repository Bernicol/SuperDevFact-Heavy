using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Application.Invoices;

/// <summary>
/// Cas d'usage signature du prototype : transforme un devis accepté en facture.
/// L'opération est exécutée dans une transaction unique car elle touche deux agrégats
/// (le devis passe à "converti", la facture est créée) : soit les deux réussissent,
/// soit aucune des deux modifications n'est persistée.
/// </summary>
public sealed class ConvertQuoteToInvoiceUseCase(
    IQuoteRepository quotes,
    IInvoiceRepository invoices,
    ICustomerRepository customers,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<InvoiceDetailsDto> ExecuteAsync(Guid quoteId, DateOnly? issueDate = null, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var quote = await quotes.GetByIdAsync(quoteId, ct)
                ?? throw new NotFoundException($"Devis {quoteId} introuvable.");

            var customer = await customers.GetByIdAsync(quote.CustomerId, ct)
                ?? throw new NotFoundException($"Client {quote.CustomerId} introuvable.");

            var effectiveIssueDate = issueDate ?? clock.Today;
            var sequence = await invoices.GetNextSequenceForYearAsync(effectiveIssueDate.Year, ct);
            var number = InvoiceNumber.Create(effectiveIssueDate.Year, sequence);

            var invoice = Invoice.CreateFromQuote(quote, number, effectiveIssueDate);
            await invoices.AddAsync(invoice, ct);

            quote.MarkConverted(invoice.Id);

            await unitOfWork.SaveChangesAsync(ct);

            return InvoiceMapper.ToDto(invoice, customer, clock.Today);
        }, cancellationToken);
    }
}
