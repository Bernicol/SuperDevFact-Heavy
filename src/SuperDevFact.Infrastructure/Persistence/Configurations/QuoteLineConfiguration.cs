using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDevFact.Domain.Quotes;
using SuperDevFact.Infrastructure.Persistence.Conversions;

namespace SuperDevFact.Infrastructure.Persistence.Configurations;

internal sealed class QuoteLineConfiguration : IEntityTypeConfiguration<QuoteLine>
{
    public void Configure(EntityTypeBuilder<QuoteLine> builder)
    {
        builder.ToTable("QuoteLines");
        builder.HasKey(l => l.Id);
        // Le domaine génère lui-même les Guid (Guid.NewGuid()) : sans ça, EF Core suppose
        // par défaut qu'une entité dont la clé est déjà renseignée existe en base dès
        // qu'elle est découverte via une collection déjà suivie, et génère un UPDATE au
        // lieu d'un INSERT pour une ligne pourtant nouvelle.
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.Position).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Detail).HasMaxLength(500);
        builder.Property(l => l.Quantity).HasPrecision(18, 3);
        builder.Property(l => l.Unit).HasMaxLength(50).IsRequired();
        builder.Property(l => l.UnitPriceHt).HasConversion(Converters.MoneyConverter).HasColumnName("UnitPriceHt").HasPrecision(18, 2);
        builder.Property(l => l.DiscountRate).HasConversion(Converters.PercentageConverter).HasColumnName("DiscountRatePercent").HasPrecision(5, 2);

        builder.OwnsOne(l => l.TaxRate, tax =>
        {
            tax.Property(t => t.Label).HasColumnName("TaxRateLabel").HasMaxLength(50).IsRequired();
            tax.Property(t => t.Rate).HasConversion(Converters.PercentageConverter).HasColumnName("TaxRatePercent").HasPrecision(5, 2);
        });
    }
}
