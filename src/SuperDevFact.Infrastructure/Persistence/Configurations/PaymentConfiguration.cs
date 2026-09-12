using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDevFact.Domain.Invoices;
using SuperDevFact.Infrastructure.Persistence.Conversions;

namespace SuperDevFact.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever(); // Guid généré par le domaine, cf. QuoteLineConfiguration

        builder.Property(p => p.Amount).HasConversion(Converters.MoneyConverter).HasColumnName("Amount").HasPrecision(18, 2);
        builder.Property(p => p.PaymentDate).IsRequired();
        builder.Property(p => p.Method).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.Reference).HasMaxLength(100);
    }
}
