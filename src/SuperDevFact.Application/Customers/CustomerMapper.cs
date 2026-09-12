using SuperDevFact.Domain.Customers;

namespace SuperDevFact.Application.Customers;

public static class CustomerMapper
{
    public static CustomerSummaryDto ToDto(Customer customer) => new(
        customer.Id,
        customer.CompanyName,
        customer.BillingAddress.Street,
        customer.BillingAddress.PostalCode,
        customer.BillingAddress.City,
        customer.BillingAddress.Country,
        customer.Contact.Name,
        customer.Contact.Email,
        customer.Contact.Phone,
        customer.Siret,
        customer.VatNumber,
        customer.DefaultPaymentTerms.Label,
        customer.IsActive);
}
