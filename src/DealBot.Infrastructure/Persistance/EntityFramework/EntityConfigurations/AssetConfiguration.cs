namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        // Table name
        builder.ToTable("Assets");

        // Primary key
        builder.HasKey(a => a.Id);

        // Properties
        ConfigureProperties(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Asset> builder)
    {
        builder.Property(a => a.FileName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(a => a.FilePath)
               .IsRequired()
               .HasMaxLength(150);

        builder.Property(a => a.FileId)
               .IsRequired(false)
               .HasMaxLength(100);
    }
}




