namespace SuperDevFact.Application.Abstractions;

/// <summary>Levée lorsqu'un cas d'usage référence une entité qui n'existe pas (ou plus).</summary>
public sealed class NotFoundException(string message) : Exception(message);
