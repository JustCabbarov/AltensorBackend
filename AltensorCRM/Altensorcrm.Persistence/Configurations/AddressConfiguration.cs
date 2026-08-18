using Altensorcrm.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AddressTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AddressType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.AddressLine1)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(300);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.StateProvince)
            .HasMaxLength(100);

        builder.Property(a => a.CityTown)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PostalCode)
            .HasMaxLength(30);

        builder.ToTable("Addresses");
    }
}
