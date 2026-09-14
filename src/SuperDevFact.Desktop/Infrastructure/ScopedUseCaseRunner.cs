using Microsoft.Extensions.DependencyInjection;

namespace SuperDevFact.Desktop.Infrastructure;

/// <summary>
/// Exécute un cas d'usage Application dans son propre scope DI (donc son propre
/// DbContext). Un cas d'usage = une unité de travail : ne jamais partager le même
/// DbContext entre deux opérations, sous peine de perturber le suivi des entités
/// nouvellement créées (cf. commentaire dans SuperDevFact.Infrastructure.DependencyInjection).
/// </summary>
public sealed class ScopedUseCaseRunner(IServiceProvider rootProvider)
{
    public async Task<TResult> RunAsync<TUseCase, TResult>(Func<TUseCase, Task<TResult>> action) where TUseCase : notnull
    {
        using var scope = rootProvider.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<TUseCase>();
        return await action(useCase);
    }

    public async Task RunAsync<TUseCase>(Func<TUseCase, Task> action) where TUseCase : notnull
    {
        using var scope = rootProvider.CreateScope();
        var useCase = scope.ServiceProvider.GetRequiredService<TUseCase>();
        await action(useCase);
    }
}
