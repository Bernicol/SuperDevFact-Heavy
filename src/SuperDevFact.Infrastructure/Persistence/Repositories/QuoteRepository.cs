using Microsoft.EntityFrameworkCore;
using SuperDevFact.Application.Abstractions;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Infrastructure.Persistence.Repositories;

internal sealed class QuoteRepository(SuperDevFactDbContext db) : IQuoteRepository
{
    public Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Quotes.Include(q => q.Lines).FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Quote>> SearchAsync(QuoteStatus? status, CancellationToken cancellationToken = default)
    {
        var query = db.Quotes.Include(q => q.Lines).AsQueryable();

        if (status is { } value)
            query = query.Where(q => q.Status == value);

        return await query.OrderByDescending(q => q.IssueDate).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Quote quote, CancellationToken cancellationToken = default) =>
        await db.Quotes.AddAsync(quote, cancellationToken);

    public async Task<int> GetNextSequenceForYearAsync(int year, CancellationToken cancellationToken = default)
    {
        var firstDayOfYear = new DateOnly(year, 1, 1);
        var firstDayOfNextYear = new DateOnly(year + 1, 1, 1);

        var count = await db.Quotes
            .CountAsync(q => q.IssueDate >= firstDayOfYear && q.IssueDate < firstDayOfNextYear, cancellationToken);

        return count + 1;
    }
}
