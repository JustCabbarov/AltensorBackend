using Altensorcrm.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.TenantId)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasMaxLength(100);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => new { u.TenantId, u.Email });

        builder.Property(u => u.Role)
            .HasMaxLength(50)
            .HasDefaultValue("User");

        builder.Property(u => u.Department)
            .HasMaxLength(150);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.HasMany(u => u.AssignedLeads)
            .WithOne(l => l.LeadOwner)
            .HasForeignKey(l => l.LeadOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedDeals)
            .WithOne(d => d.DealOwner)
            .HasForeignKey(d => d.DealOwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedContacts)
            .WithOne(c => c.AssignedUser)
            .HasForeignKey(c => c.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.AssignedTasks)
            .WithOne(t => t.AssignedUser)
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.CreatedNotes)
            .WithOne(n => n.CreatedBy)
            .HasForeignKey(n => n.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Users");
    }
}
