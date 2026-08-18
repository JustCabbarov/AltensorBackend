using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Salutation)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(l => l.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.MobileNo)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(l => l.Gender)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Website)
            .HasMaxLength(300);

        builder.Property(l => l.NoOfEmployees)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(l => l.AnnualRevenue)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0m);

        builder.Property(l => l.Industry)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(LeadStatus.New);

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(l => l.LeadOwner)
            .WithMany(u => u.AssignedLeads)
            .HasForeignKey(l => l.LeadOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Territory)
            .WithMany(t => t.Leads)
            .HasForeignKey(l => l.TerritoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(l => l.Notes)
            .WithOne(n => n.Lead)
            .HasForeignKey(n => n.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Tasks)
            .WithOne(t => t.Lead)
            .HasForeignKey(t => t.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.CallLogs)
            .WithOne(c => c.Lead)
            .HasForeignKey(c => c.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Comments)
            .WithOne()
            .HasForeignKey(c => c.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Attachments)
            .WithOne()
            .HasForeignKey(a => a.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Notifications)
            .WithOne(n => n.Lead)
            .HasForeignKey(n => n.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("Leads");
    }
}
