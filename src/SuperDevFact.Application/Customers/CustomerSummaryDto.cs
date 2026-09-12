namespace SuperDevFact.Application.Customers;

public sealed record CustomerSummaryDto(
    Guid Id,
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
    string PaymentTermsLabel,
    bool IsActive);
