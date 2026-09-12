namespace SuperDevFact.Application.Customers;

public sealed record CreateCustomerRequest(
    string CompanyName,
    string Street,
    string PostalCode,
    string City,
    string Country,
    string ContactName,
    string ContactEmail,
    string? ContactPhone,
    string? Siret,
    string? VatNumber,
    int PaymentTermsDays);
