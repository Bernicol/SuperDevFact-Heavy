using Microsoft.EntityFrameworkCore;
using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Infrastructure.Persistence.Repositories;

internal sealed class InvoiceRepository(SuperDevFactDbContext db) : IInvoiceRepository
{
    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Invoices.Include(i => i.Lines).Include(i => i.Payments).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Invoice>> SearchAsync(InvoiceStatus? status, CancellationToken cancellationToken = default)
    {
        var query = db.Invoices.Include(i => i.Lines).Include(i => i.Payments).AsQueryable();

        if (status is { } value)
            query = query.Where(i => i.Status == value);

        return await query.OrderByDescending(i => i.IssueDate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default) =>
        await db.Invoices.AddAsync(invoice, cancellationToken);

    public async Task<int> GetNextSequenceForYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var firstDayOfYear = new DateOnly(year, 1, 1);
        var firstDayOfNextYear = new DateOnly(year + 1, 1, 1);

        var count = await db.Invoices
            .CountAsync(i => i.IssueDate >= firstDayOfYear && i.IssueDate < firstDayOfNextYear, cancellationToken);

        return count + 1;
    }
}
