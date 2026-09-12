using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Application.Customers;

/// <summary>Recherche instantanée de clients, utilisée par le sélecteur de client du devis et la Command Palette.</summary>
public sealed class SearchCustomersUseCase(ICustomerRepository customers)
{
    public async Task<IReadOnlyList<CustomerSummaryDto>> ExecuteAsync(string? searchText, CancellationToken cancellationToken = default)
    {
        var results = await customers.SearchAsync(searchText, cancellationToken);
        return results.Select(CustomerMapper.ToDto).ToList();
    }
}
