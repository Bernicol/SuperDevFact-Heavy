using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SuperDevFact.Domain.Customers;

namespace SuperDevFact.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); // Guid généré par le domaine, cf. QuoteLineConfiguration

        builder.Property(c => c.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Siret).HasMaxLength(20);
        builder.Property(c => c.VatNumber).HasMaxLength(20);
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.CreatedAtUtc).IsRequired();

        builder.OwnsOne(c => c.BillingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200).IsRequired();
            address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20).IsRequired();
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
        });

        builder.OwnsOne(c => c.Contact, contact =>
        {
            contact.Property(x => x.Name).HasColumnName("ContactName").HasMaxLength(200).IsRequired();
            contact.Property(x => x.Email).HasColumnName("ContactEmail").HasMaxLength(200).IsRequired();
            contact.Property(x => x.Phone).HasColumnName("ContactPhone").HasMaxLength(50);
        });

        builder.OwnsOne(c => c.DefaultPaymentTerms, terms =>
        {
            terms.Property(x => x.Kind).HasColumnName("PaymentTermsKind").HasConversion<string>().HasMaxLength(20).IsRequired();
            terms.Property(x => x.Days).HasColumnName("PaymentTermsDays").IsRequired();
            terms.Property(x => x.Label).HasColumnName("PaymentTermsLabel").HasMaxLength(50).IsRequired();
        });

        builder.HasIndex(c => c.CompanyName);
    }
}
