namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        // Table name
        builder.ToTable("Stores");

        // Primary key
        builder.HasKey(s => s.Id);

        // Properties
        ConfigureProperties(builder);

        // Relationships
        ConfigureRelationships(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Store> builder)
    {
        builder.Property(s => s.Name)
               .IsRequired(false)
               .HasMaxLength(80);

        builder.Property(s => s.Website)
               .IsRequired(false)
               .HasMaxLength(50);

        builder.Property(s => s.MiniAppUrl)
               .IsRequired(false)
               .HasMaxLength(200);

        builder.Property(s => s.Description)
               .IsRequired(false)
               .HasMaxLength(500);

        builder.Property(s => s.Channel)
               .IsRequired(false)
               .HasMaxLength(500);
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Store> builder)
    {
        builder.HasOne(s => s.Contact)
            .WithOne()
            .HasForeignKey<Store>(s => s.ContactId);

        builder.HasOne(s => s.Address)
            .WithOne()
            .HasForeignKey<Store>(s => s.AddressId);

        builder.HasOne(s => s.Image)
            .WithOne()
            .HasForeignKey<Store>(s => s.AssetId);

        builder.HasMany(s => s.Reviews)
            .WithOne(r => r.Store)
            .HasForeignKey(r => r.StoreId);
    }
}