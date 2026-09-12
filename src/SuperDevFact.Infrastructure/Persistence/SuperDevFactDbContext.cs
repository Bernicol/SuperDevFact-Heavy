using Microsoft.EntityFrameworkCore;
using SuperDevFact.Domain.Customers;
using SuperDevFact.Domain.Invoices;
using SuperDevFact.Domain.Quotes;

namespace SuperDevFact.Infrastructure.Persistence;

/// <summary>
/// Point d'entrée EF Core. Volontairement le seul endroit de tout le projet qui connaît
/// EF Core dans le sens "assemblage du modèle" : le domaine n'a aucune référence à cette
/// classe.
/// </summary>
public sealed class SuperDevFactDbContext(DbContextOptions<SuperDevFactDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Quote> Quotes => Set<Quote>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuperDevFactDbContext).Assembly);
    }
}
