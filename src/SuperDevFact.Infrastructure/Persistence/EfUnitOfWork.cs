using SuperDevFact.Application.Abstractions;

namespace SuperDevFact.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(SuperDevFactDbContext db) : IUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        // Une transaction explicite est nécessaire ici car certaines opérations (ex. la
        // conversion d'un devis en facture) modifient deux agrégats distincts : soit les
        // deux écritures réussissent, soit aucune n'est persistée.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var result = await operation(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return result;
    }
}
