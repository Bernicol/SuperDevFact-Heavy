using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Application.Quotes;

public sealed class SearchQuotesUseCase(IQuoteRepository quotes, ICustomerRepository customers)
{
    public async Task<IReadOnlyList<QuoteSummaryDto>> ExecuteAsync(
        string? searchText, QuoteStatus? status, CancellationToken cancellationToken = default)
    {
        var results = await quotes.SearchAsync(status, cancellationToken);
        var customerNamesById = new Dictionary<Guid, string>();

        var summaries = new List<QuoteSummaryDto>(results.Count);
        foreach (var quote in results)
        {
            if (!customerNamesById.TryGetValue(quote.CustomerId, out var customerName))
            {
                var customer = await customers.GetByIdAsync(quote.CustomerId, cancellationToken);
                customerName = customer?.CompanyName ?? "Client inconnu";
                customerNamesById[quote.CustomerId] = customerName;
            }

            summaries.Add(new QuoteSummaryDto(
                quote.Id, quote.Number.Value, customerName, quote.IssueDate,
                quote.CalculateTotals().SubtotalHt.Amount, quote.Status.ToString()));
        }

        if (string.IsNullOrWhiteSpace(searchText))
            return summaries;

        return summaries
            .Where(s => s.Number.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                     || s.CustomerName.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
