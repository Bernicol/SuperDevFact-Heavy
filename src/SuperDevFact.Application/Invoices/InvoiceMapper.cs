using SuperDevFact.Application.Customers;
using SuperDevFact.Application.Documents;
using SuperDevFact.Domain.Customers;
using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Application.Invoices;

public static class InvoiceMapper
{
    public static InvoiceDetailsDto ToDto(Invoice invoice, Customer customer, DateOnly asOfDate)
    {
        var totals = invoice.CalculateTotals();

        return new InvoiceDetailsDto(
            invoice.Id,
            invoice.Number.Value,
            CustomerMapper.ToDto(customer),
            invoice.SourceQuoteId,
            invoice.IssueDate,
            invoice.DueDate,
            invoice.PaymentTerms.Label,
            invoice.ClientReference,
            invoice.InternalNotes,
            invoice.ClientMessage,
            invoice.Status.ToString(),
            invoice.IsOverdue(asOfDate),
            invoice.GlobalDiscountRate.Value,
            invoice.Lines.Select(ToLineDto).ToList(),
            invoice.Payments.Select(ToPaymentDto).ToList(),
            invoice.AmountPaid().Amount,
            invoice.Balance().Amount,
            DocumentTotalsMapper.ToDto(totals),
            invoice.CreatedAtUtc,
            invoice.LastModifiedAtUtc);
    }

    public static InvoiceLineDto ToLineDto(InvoiceLine line) => new(
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

    public static PaymentDto ToPaymentDto(Payment payment) => new(
        payment.Id, payment.Amount.Amount, payment.PaymentDate, payment.Method.ToString(), payment.Reference);
}
