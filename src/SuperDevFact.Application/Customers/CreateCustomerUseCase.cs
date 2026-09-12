using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Customers;
using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Application.Customers;

public sealed class CreateCustomerUseCase(ICustomerRepository customers, IUnitOfWork unitOfWork)
{
    public async Task<CustomerSummaryDto> ExecuteAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = new Customer(
            request.CompanyName,
            new Address(request.Street, request.PostalCode, request.City, request.Country),
            new ContactInfo(request.ContactName, request.ContactEmail, request.ContactPhone),
            PaymentTerms.NetDays(request.PaymentTermsDays),
            request.Siret,
            request.VatNumber);

        await customers.AddAsync(customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerMapper.ToDto(customer);
    }
}
