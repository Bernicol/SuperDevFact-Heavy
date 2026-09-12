using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SuperDevFact.Infrastructure.Persistence;

/// <summary>
/// Utilisée uniquement par les outils `dotnet ef` (génération de migrations) au moment
/// du design : l'application elle-même configure son DbContext via
/// <see cref="DependencyInjection.AddInfrastructure"/> avec la chaîne de connexion réelle.
/// </summary>
public sealed class SuperDevFactDbContextFactory : IDesignTimeDbContextFactory<SuperDevFactDbContext>
{
    public SuperDevFactDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SUPERDEVFACT_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=superdevfact;Username=postgres;Password=171717";

        var optionsBuilder = new DbContextOptionsBuilder<SuperDevFactDbContext>()
            .UseNpgsql(connectionString);

        return new SuperDevFactDbContext(optionsBuilder.Options);
    }
}
