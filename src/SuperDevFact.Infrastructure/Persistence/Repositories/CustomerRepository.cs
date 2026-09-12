using Microsoft.EntityFrameworkCore;
using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Customers;

namespace SuperDevFact.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(SuperDevFactDbContext db) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Customer>> SearchAsync(string? searchText, CancellationToken cancellationToken = default)
    {
        var query = db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var pattern = $"%{searchText.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.CompanyName, pattern));
        }

        return await query.OrderBy(c => c.CompanyName).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default) =>
        await db.Customers.AddAsync(customer, cancellationToken);
}
