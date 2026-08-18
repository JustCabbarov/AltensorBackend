using Altensorcrm.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Salutation)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.EmailAddress)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.MobileNo)
            .HasMaxLength(30);

        builder.Property(c => c.Gender)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.CompanyName)
            .HasMaxLength(200);

        builder.Property(c => c.Designation)
            .HasMaxLength(150);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(c => c.Address)
            .WithMany(a => a.Contacts)
            .HasForeignKey(c => c.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Organization)
            .WithMany(o => o.Contacts)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.AssignedUser)
            .WithMany(u => u.AssignedContacts)
            .HasForeignKey(c => c.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Deals)
            .WithOne(d => d.Contact)
            .HasForeignKey(d => d.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("Contacts");
    }
}
