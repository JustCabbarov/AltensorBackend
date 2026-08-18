using Altensorcrm.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrganizationName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.AnnualRevenue)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(o => o.Website)
            .HasMaxLength(300);

        builder.Property(o => o.NoOfEmployees)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(o => o.Industry)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(o => o.Territory)
            .WithMany(t => t.Organizations)
            .HasForeignKey(o => o.TerritoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Address)
            .WithMany(a => a.Organizations)
            .HasForeignKey(o => o.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Contacts)
            .WithOne(c => c.Organization)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Deals)
            .WithOne(d => d.Organization)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("Organizations");
    }
}
