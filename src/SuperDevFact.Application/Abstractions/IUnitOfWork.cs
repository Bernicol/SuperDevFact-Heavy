namespace SuperDevFact.Application.Abstractions;

/// <summary>
/// Frontière transactionnelle exposée au niveau Application. L'implémentation concrète
/// (Infrastructure/EF Core) sait ce qu'est une transaction SQL ; l'Application se
/// contente de dire "ceci doit être atomique".
/// </summary>
public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
