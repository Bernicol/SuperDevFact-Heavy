namespace SuperDevFact.Domain.Customers;

public sealed record Address(string Street, string PostalCode, string City, string Country = "France")
{
    public override string ToString() => $"{Street}, {PostalCode} {City}, {Country}";
}
