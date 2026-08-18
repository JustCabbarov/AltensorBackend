using Altensorcrm.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altensorcrm.Persistence.Configurations;

public class TerritoryConfiguration : IEntityTypeConfiguration<Territory>
{
    public void Configure(EntityTypeBuilder<Territory> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TerritoryName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.IsGroup)
            .HasDefaultValue(false);

        builder.HasOne(t => t.TerritoryManager)
            .WithMany()
            .HasForeignKey(t => t.TerritoryManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ParentTerritory)
            .WithMany(t => t.ChildTerritories)
            .HasForeignKey(t => t.ParentTerritoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Territories");
    }
}
