using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Application.Invoices;

public sealed class GetInvoiceForViewingUseCase(IInvoiceRepository invoices, ICustomerRepository customers, IClock clock)
{
    public async Task<InvoiceDetailsDto> ExecuteAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await invoices.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException($"Facture {invoiceId} introuvable.");

        var customer = await customers.GetByIdAsync(invoice.CustomerId, cancellationToken)
            ?? throw new NotFoundException($"Client {invoice.CustomerId} introuvable.");

        return InvoiceMapper.ToDto(invoice, customer, clock.Today);
    }
}
