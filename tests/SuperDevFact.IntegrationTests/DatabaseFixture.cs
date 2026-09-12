using Microsoft.EntityFrameworkCore;
using SuperDevFact.Infrastructure.Persistence;
using Xunit;

namespace SuperDevFact.IntegrationTests;

/// <summary>
/// Pointe vers une base PostgreSQL réelle et dédiée aux tests (superdevfact_test), distincte
/// de la base de démo (superdevfact). Applique les migrations une fois, puis permet de
/// vider les données entre chaque test pour garantir leur indépendance.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    public const string ConnectionString =
        "Host=localhost;Port=5432;Database=superdevfact_test;Username=postgres;Password=171717";

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public static SuperDevFactDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SuperDevFactDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new SuperDevFactDbContext(options);
    }

    /// <summary>Vide toutes les tables métier entre deux tests (l'historique des migrations est conservé).</summary>
    public static async Task ResetAsync()
    {
        await using var db = CreateContext();
        await db.Database.ExecuteSqlRawAsync(
            """
            TRUNCATE TABLE "Payments", "InvoiceLines", "Invoices", "QuoteLines", "Quotes", "Customers" RESTART IDENTITY CASCADE;
            """);
    }
}
