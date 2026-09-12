namespace SuperDevFact.Application.Abstractions;

/// <summary>
/// Abstraction du temps courant. Permet aux cas d'usage de rester déterministes et
/// testables (un test peut injecter une horloge figée) sans dépendre de DateTime.Now.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}
