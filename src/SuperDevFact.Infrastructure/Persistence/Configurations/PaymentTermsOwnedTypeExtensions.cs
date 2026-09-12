using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDevFact.Domain.Documents;

namespace SuperDevFact.Infrastructure.Persistence.Configurations;

/// <summary>Mapping partagé des conditions de paiement, utilisé par Quote et Invoice.</summary>
internal static class PaymentTermsOwnedTypeExtensions
{
    public static void OwnsPaymentTerms<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        System.Linq.Expressions.Expression<Func<TEntity, PaymentTerms?>> navigationExpression)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, terms =>
        {
            terms.Property(x => x.Kind).HasColumnName("PaymentTermsKind").HasConversion<string>().HasMaxLength(20).IsRequired();
            terms.Property(x => x.Days).HasColumnName("PaymentTermsDays").IsRequired();
            terms.Property(x => x.Label).HasColumnName("PaymentTermsLabel").HasMaxLength(50).IsRequired();
        });
    }
}
