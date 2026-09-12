using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Common;

namespace SuperDevFact.Application.Invoices;

public sealed class RecordPaymentUseCase(IInvoiceRepository invoices, ICustomerRepository customers, IUnitOfWork unitOfWork, IClock clock)
{
    public async Task<InvoiceDetailsDto> ExecuteAsync(Guid invoiceId, RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var invoice = await invoices.GetByIdAsync(invoiceId, cancellationToken)
            ?? throw new NotFoundException($"Facture {invoiceId} introuvable.");

        var customer = await customers.GetByIdAsync(invoice.CustomerId, cancellationToken)
            ?? throw new NotFoundException($"Client {invoice.CustomerId} introuvable.");

        invoice.RecordPayment(new Money(request.Amount), request.PaymentDate, request.Method, request.Reference);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return InvoiceMapper.ToDto(invoice, customer, clock.Today);
    }
}
