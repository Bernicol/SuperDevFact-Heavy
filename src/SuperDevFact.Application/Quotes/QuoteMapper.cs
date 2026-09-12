using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Documents;
using SuperDevFact.Domain.Customers;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Application.Quotes;

public static class QuoteMapper
{
    public static QuoteDetailsDto ToDto(Quote quote, Customer customer)
    {
        var totals = quote.CalculateTotals();

        return new QuoteDetailsDto(
            quote.Id,
            quote.Number.Value,
            CustomerMapper.ToDto(customer),
            quote.IssueDate,
            quote.ExpiryDate,
            quote.ValidityDays,
            quote.PaymentTerms.Label,
            quote.ClientReference,
            quote.InternalNotes,
            quote.ClientMessage,
            quote.Status.ToString(),
            quote.ConvertedInvoiceId,
            quote.GlobalDiscountRate.Value,
            quote.Lines.Select(ToLineDto).ToList(),
            DocumentTotalsMapper.ToDto(totals),
            quote.CreatedAtUtc,
            quote.LastModifiedAtUtc);
    }

    public static QuoteLineDto ToLineDto(QuoteLine line) => new(
        line.Id,
        line.Position,
        line.Description,
        line.Detail,
        line.Quantity,
        line.Unit,
        line.UnitPriceHt.Amount,
        line.DiscountRate.Value,
        line.TaxRate.Rate.Value,
        line.TaxRate.Label,
        line.NetAmountHt.Amount);
}
