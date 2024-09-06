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
                       .IsRequired()
                       .HasMaxLength(200);

        builder.Property(s => s.CashBackPersentage)
               .HasColumnType("decimal(5,2)")
               .IsRequired();

        builder.Property(s => s.IsActive)
               .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Store> builder)
    {
        builder.HasOne(s => s.Contact)
                       .WithOne()
                       .HasForeignKey<Store>(s => s.ContactId);

        builder.HasOne(s => s.Address)
               .WithOne()
               .HasForeignKey<Store>(s => s.AddressId);

        builder.HasOne(s => s.Owner)
               .WithOne(u => u.Store)
               .HasForeignKey<Store>(s => s.OwnerId);

        builder.HasOne(s => s.Image)
               .WithOne()
               .HasForeignKey<Store>(s => s.AssetId);

        builder.HasMany(s => s.Transactions)
               .WithOne(t => t.Store)
               .HasForeignKey(t => t.StoreId);

        builder.HasMany(s => s.Reviews)
               .WithOne(r => r.Store)
               .HasForeignKey(r => r.StoreId);
    }
}