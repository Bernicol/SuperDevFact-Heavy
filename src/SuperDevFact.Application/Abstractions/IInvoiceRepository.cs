using SuperDevFact.Domain.Invoices;

namespace SuperDevFact.Application.Abstractions;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Filtre uniquement par statut : le filtrage textuel (numéro, nom client) est fait
    /// au niveau Application, une fois les noms de clients résolus, pour éviter de
    /// dépendre d'une traduction SQL fragile sur des value objects.
    /// </summary>
    Task<IReadOnlyList<Invoice>> SearchAsync(InvoiceStatus? status, CancellationToken cancellationToken = default);

    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>Prochain numéro de séquence pour l'année donnée (ex. 83 pour générer FAC-2026-0083).</summary>
    Task<int> GetNextSequenceForYearAsync(int year, CancellationToken cancellationToken = default);
}
