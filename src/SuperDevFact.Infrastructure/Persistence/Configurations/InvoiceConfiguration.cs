using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDevFact.Domain.Customers;
using SuperDevFact.Domain.Invoices;
using SuperDevFact.Infrastructure.Persistence.Conversions;

namespace SuperDevFact.Infrastructure.Persistence.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever(); // Guid généré par le domaine, cf. QuoteLineConfiguration

        builder.Property(i => i.Number).HasConversion(Converters.InvoiceNumberConverter).HasColumnName("Number").HasMaxLength(20).IsRequired();
        builder.HasIndex(i => i.Number).IsUnique();

        builder.Property(i => i.CustomerId).IsRequired();
        builder.HasOne<Customer>().WithMany().HasForeignKey(i => i.CustomerId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(i => i.SourceQuoteId);

        builder.Property(i => i.IssueDate).IsRequired();
        builder.Property(i => i.DueDate).IsRequired();
        builder.Property(i => i.ClientReference).HasMaxLength(100);
        builder.Property(i => i.InternalNotes).HasMaxLength(2000);
        builder.Property(i => i.ClientMessage).HasMaxLength(2000);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.CreatedAtUtc).IsRequired();
        builder.Property(i => i.LastModifiedAtUtc).IsRequired();
        builder.Property(i => i.GlobalDiscountRate)
            .HasConversion(Converters.PercentageConverter)
            .HasColumnName("GlobalDiscountRatePercent")
            .HasPrecision(5, 2);

        builder.OwnsPaymentTerms(i => i.PaymentTerms);

        builder.HasMany(i => i.Lines)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(i => i.Payments)
            .WithOne()
            .HasForeignKey("InvoiceId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Payments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(i => i.Status);
    }
}
