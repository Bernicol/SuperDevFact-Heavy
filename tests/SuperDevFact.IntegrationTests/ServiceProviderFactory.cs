using Microsoft.Extensions.DependencyInjection;
using SuperDevFact.Application;
using SuperDevFact.Application.Abstractions;
using SuperDevFact.Infrastructure;

namespace SuperDevFact.IntegrationTests;

/// <summary>
/// Assemble un conteneur DI identique à celui du Desktop (Application + Infrastructure
/// branchées sur la vraie base PostgreSQL de test), avec une horloge figée pour des
/// assertions déterministes.
/// </summary>
internal static class ServiceProviderFactory
{
    public static ServiceProvider Create(DateOnly today)
    {
        var services = new ServiceCollection()
            .AddApplication()
            .AddInfrastructure(DatabaseFixture.ConnectionString);

        services.AddSingleton<IClock>(new FixedClock(today));

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Exécute un cas d'usage dans son propre scope DI, donc avec son propre DbContext.
    /// C'est le même principe que doit suivre le Desktop : une exécution de cas d'usage =
    /// une unité de travail = un DbContext dédié, jamais partagé entre deux opérations.
    /// Réutiliser le même DbContext pour plusieurs appels successifs perturbe le suivi
    /// des entités nouvellement ajoutées à un agrégat déjà persisté.
    /// </summary>
    public static async Task<TResult> RunAsync<TUseCase, TResult>(
        this ServiceProvider root, Func<TUseCase, Task<TResult>> action) where TUseCase : notnull
    {
        using var scope = root.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<TUseCase>();
        return await action(useCase);
    }
}
