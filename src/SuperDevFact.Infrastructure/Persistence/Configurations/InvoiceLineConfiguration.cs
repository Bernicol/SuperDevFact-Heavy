using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDevFact.Domain.Invoices;
using SuperDevFact.Infrastructure.Persistence.Conversions;

namespace SuperDevFact.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever(); // Guid généré par le domaine, cf. QuoteLineConfiguration

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
