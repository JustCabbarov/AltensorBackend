using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.ChooseExistingOrganization)
            .HasDefaultValue(false);

        builder.Property(d => d.ChooseExistingContact)
            .HasDefaultValue(false);

        builder.Property(d => d.OrganizationName)
            .HasMaxLength(200);

        builder.Property(d => d.Website)
            .HasMaxLength(300);

        builder.Property(d => d.NoOfEmployees)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(d => d.AnnualRevenue)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(d => d.Industry)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Salutation)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(d => d.FirstName)
            .HasMaxLength(100);

        builder.Property(d => d.LastName)
            .HasMaxLength(100);

        builder.Property(d => d.PrimaryEmail)
            .HasMaxLength(200);

        builder.Property(d => d.PrimaryMobileNo)
            .HasMaxLength(30);

        builder.Property(d => d.Gender)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(DealStatus.Qualification);

        builder.Property(d => d.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(d => d.DealOwner)
            .WithMany(u => u.AssignedDeals)
            .HasForeignKey(d => d.DealOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Territory)
            .WithMany(t => t.Deals)
            .HasForeignKey(d => d.TerritoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.SourceLead)
            .WithMany()
            .HasForeignKey(d => d.SourceLeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Organization)
            .WithMany(o => o.Deals)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(d => d.Contact)
            .WithMany(c => c.Deals)
            .HasForeignKey(d => d.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.Notes)
            .WithOne(n => n.Deal)
            .HasForeignKey(n => n.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Tasks)
            .WithOne(t => t.Deal)
            .HasForeignKey(t => t.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.CallLogs)
            .WithOne(c => c.Deal)
            .HasForeignKey(c => c.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Comments)
            .WithOne()
            .HasForeignKey(c => c.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Attachments)
            .WithOne()
            .HasForeignKey(a => a.DealId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Notifications)
            .WithOne(n => n.Deal)
            .HasForeignKey(n => n.DealId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("Deals");
    }
}
