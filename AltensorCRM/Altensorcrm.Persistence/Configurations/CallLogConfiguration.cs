using Altensorcrm.Domain.Entity;
using Altensorcrm.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class CallLogConfiguration : IEntityTypeConfiguration<CallLog>
{
    public void Configure(EntityTypeBuilder<CallLog> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CallType.Incoming);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(CallStatus.Completed);

        builder.Property(c => c.FromNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.ToNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.DurationInSeconds)
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedOn)
            .HasDefaultValueSql("NOW()");

        builder.HasOne(c => c.CallReceivedBy)
            .WithMany()
            .HasForeignKey(c => c.CallReceivedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.CallerUser)
            .WithMany()
            .HasForeignKey(c => c.CallerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("CallLogs");
    }
}
