namespace SuperDevFact.Domain.Invoices;

/// <summary>
/// Cycle de vie d'une facture. Une facture est créée déjà "Sent" (elle provient d'un
/// devis accepté et est immédiatement émise) ; "Overdue" n'est volontairement pas un
/// état stocké mais une notion calculée (cf. <see cref="Invoice.IsOverdue"/>), car elle
/// dépend de la date du jour et non d'une décision explicite.
/// </summary>
public enum InvoiceStatus
{
    Draft,
    Sent,
    PartiallyPaid,
    Paid,
    Cancelled
}
