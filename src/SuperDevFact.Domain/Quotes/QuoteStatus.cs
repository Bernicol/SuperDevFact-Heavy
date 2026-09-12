namespace SuperDevFact.Domain.Quotes;

/// <summary>
/// Cycle de vie d'un devis. La transformation en facture n'est pas un statut en tant
/// que tel : elle est représentée par <see cref="Quote.ConvertedInvoiceId"/>, ce qui
/// permet de savoir qu'un devis "Accepté" a été transformé sans mélanger deux concepts
/// distincts (le statut du devis et le fait qu'une facture existe derrière).
/// </summary>
public enum QuoteStatus
{
    Draft,
    Sent,
    Accepted,
    Declined,
    Expired
}
