namespace SuperDevFact.Application.Invoices;

/// <summary>Ligne affichée dans la liste globale des paiements (écran "Paiements").</summary>
public sealed record PaymentListItemDto(
    Guid Id,
    Guid InvoiceId,
    string InvoiceNumber,
    string CustomerName,
    decimal Amount,
    DateOnly PaymentDate,
    string Method,
    string? Reference);
