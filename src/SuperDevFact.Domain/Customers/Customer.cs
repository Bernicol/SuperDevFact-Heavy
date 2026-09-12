using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Domain.Customers;

/// <summary>Fiche client : agrégat racine, indépendant de tout devis ou facture.</summary>
public sealed class Customer
{
    public Guid Id { get; }
    public string CompanyName { get; private set; }
    public Address BillingAddress { get; private set; }
    public ContactInfo Contact { get; private set; }
    public string? Siret { get; private set; }
    public string? VatNumber { get; private set; }
    public PaymentTerms DefaultPaymentTerms { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }

    private Customer()
    {
        CompanyName = string.Empty;
        BillingAddress = new Address(string.Empty, string.Empty, string.Empty);
        Contact = new ContactInfo(string.Empty, string.Empty, null);
        DefaultPaymentTerms = PaymentTerms.Immediate;
    } // réservé à EF Core

    public Customer(
        string companyName,
        Address billingAddress,
        ContactInfo contact,
        PaymentTerms defaultPaymentTerms,
        string? siret = null,
        string? vatNumber = null)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Le nom de la société est obligatoire.", nameof(companyName));

        Id = Guid.NewGuid();
        CompanyName = companyName;
        BillingAddress = billingAddress;
        Contact = contact;
        DefaultPaymentTerms = defaultPaymentTerms;
        Siret = siret;
        VatNumber = vatNumber;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void UpdateProfile(
        string companyName,
        Address billingAddress,
        ContactInfo contact,
        PaymentTerms defaultPaymentTerms,
        string? siret,
        string? vatNumber)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new ArgumentException("Le nom de la société est obligatoire.", nameof(companyName));

        CompanyName = companyName;
        BillingAddress = billingAddress;
        Contact = contact;
        DefaultPaymentTerms = defaultPaymentTerms;
        Siret = siret;
        VatNumber = vatNumber;
    }

    public void Deactivate() => IsActive = false;

    public void Reactivate() => IsActive = true;
}
