using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Application.Invoices;

/// <summary>Liste tous les paiements enregistrés, toutes factures confondues, pour l'écran "Paiements".</summary>
public sealed class ListPaymentsUseCase(IInvoiceRepository invoices, ICustomerRepository customers)
{
    public async Task<IReadOnlyList<PaymentListItemDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var allInvoices = await invoices.SearchAsync(null, cancellationToken);
        var customerNamesById = new Dictionary<Guid, string>();
        var results = new List<PaymentListItemDto>();

        foreach (var invoice in allInvoices)
        {
            if (invoice.Payments.Count == 0)
                continue;

            if (!customerNamesById.TryGetValue(invoice.CustomerId, out var customerName))
            {
                var customer = await customers.GetByIdAsync(invoice.CustomerId, cancellationToken);
                customerName = customer?.CompanyName ?? "Client inconnu";
                customerNamesById[invoice.CustomerId] = customerName;
            }

            foreach (var payment in invoice.Payments)
            {
                results.Add(new PaymentListItemDto(
                    payment.Id, invoice.Id, invoice.Number.Value, customerName,
                    payment.Amount.Amount, payment.PaymentDate, payment.Method.ToString(), payment.Reference));
            }
        }

        return results.OrderByDescending(p => p.PaymentDate).ToList();
    }
}
