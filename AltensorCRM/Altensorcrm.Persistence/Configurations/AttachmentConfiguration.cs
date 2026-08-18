using Altensorcrm.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(a => a.FilePath)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.FileSize)
            .IsRequired();

        builder.Property(a => a.UploadedAt)
            .HasDefaultValueSql("NOW()");

        builder.HasIndex(a => a.LeadId);
        builder.HasIndex(a => a.DealId);
        builder.HasIndex(a => a.TaskItemId);

        builder.ToTable("Attachments");
    }
}
