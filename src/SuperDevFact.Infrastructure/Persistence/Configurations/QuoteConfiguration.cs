using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDevFact.Domain.Customers;
using SuperDevFact.Domain.Quotes;
using SuperDevFact.Infrastructure.Persistence.Conversions;

namespace SuperDevFact.Infrastructure.Persistence.Configurations;

internal sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ValueGeneratedNever(); // Guid généré par le domaine, cf. QuoteLineConfiguration

        builder.Property(q => q.Number).HasConversion(Converters.QuoteNumberConverter).HasColumnName("Number").HasMaxLength(20).IsRequired();
        builder.HasIndex(q => q.Number).IsUnique();

        builder.Property(q => q.CustomerId).IsRequired();
        builder.HasOne<Customer>().WithMany().HasForeignKey(q => q.CustomerId).OnDelete(DeleteBehavior.Restrict);

        builder.Property(q => q.IssueDate).IsRequired();
        builder.Property(q => q.ValidityDays).IsRequired();
        builder.Property(q => q.ClientReference).HasMaxLength(100);
        builder.Property(q => q.InternalNotes).HasMaxLength(2000);
        builder.Property(q => q.ClientMessage).HasMaxLength(2000);
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(q => q.ConvertedInvoiceId);
        builder.Property(q => q.CreatedAtUtc).IsRequired();
        builder.Property(q => q.LastModifiedAtUtc).IsRequired();
        builder.Property(q => q.GlobalDiscountRate)
            .HasConversion(Converters.PercentageConverter)
            .HasColumnName("GlobalDiscountRatePercent")
            .HasPrecision(5, 2);

        builder.OwnsPaymentTerms(q => q.PaymentTerms);

        builder.HasMany(q => q.Lines)
            .WithOne()
            .HasForeignKey("QuoteId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(q => q.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(q => q.Status);
    }
}
