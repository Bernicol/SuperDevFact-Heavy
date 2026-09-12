using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Application.Invoices;

public sealed class SearchInvoicesUseCase(IInvoiceRepository invoices, ICustomerRepository customers, IClock clock)
{
    public async Task<IReadOnlyList<InvoiceSummaryDto>> ExecuteAsync(
        string? searchText, InvoiceStatus? status, CancellationToken cancellationToken = default)
    {
        var results = await invoices.SearchAsync(status, cancellationToken);
        var customerNamesById = new Dictionary<Guid, string>();

        var summaries = new List<InvoiceSummaryDto>(results.Count);
        foreach (var invoice in results)
        {
            if (!customerNamesById.TryGetValue(invoice.CustomerId, out var customerName))
            {
                var customer = await customers.GetByIdAsync(invoice.CustomerId, cancellationToken);
                customerName = customer?.CompanyName ?? "Client inconnu";
                customerNamesById[invoice.CustomerId] = customerName;
            }

            summaries.Add(new InvoiceSummaryDto(
                invoice.Id, invoice.Number.Value, customerName, invoice.IssueDate,
                invoice.CalculateTotals().TotalTtc.Amount, invoice.Status.ToString(), invoice.IsOverdue(clock.Today)));
        }

        if (string.IsNullOrWhiteSpace(searchText))
            return summaries;

        return summaries
            .Where(s => s.Number.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                     || s.CustomerName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
