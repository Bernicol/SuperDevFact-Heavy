using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Application.Abstractions;

public interface IQuoteRepository
{
    Task<Quote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filtre uniquement par statut : le filtrage textuel (numéro, nom client) est fait
    /// au niveau Application, une fois les noms de clients résolus, pour éviter de
    /// dépendre d'une traduction SQL fragile sur des value objects.
    /// </summary>
    Task<IReadOnlyList<Quote>> SearchAsync(QuoteStatus? status, CancellationToken cancellationToken = default);

    Task AddAsync(Quote quote, CancellationToken cancellationToken = default);

    /// <summary>Prochain numéro de séquence pour l'année donnée (ex. 43 pour générer DEV-2026-0043).</summary>
    Task<int> GetNextSequenceForYearAsync(int year, CancellationToken cancellationToken = default);
}
