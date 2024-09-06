namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.User> builder)
    {
        // Table name
        builder.ToTable("Users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Properties
        ConfigureProperties(builder);

        // Relationships
        ConfigureRelationships(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.FirstName)
               .HasMaxLength(30);

        builder.Property(u => u.LastName)
               .HasMaxLength(30);

        builder.Property(u => u.Username)
               .HasMaxLength(22);

        builder.Property(u => u.Balance)
               .HasColumnType("decimal(18,2)");

        builder.Property(u => u.Role)
               .IsRequired();

        builder.Property(u => u.TelegramId)
               .IsRequired();

        builder.Property(u => u.State)
               .IsRequired();

        builder.Property(u => u.IsActive)
               .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<User> builder)
    {
        builder.HasOne(u => u.Contact)
               .WithOne()
               .HasForeignKey<User>(u => u.ContactId);

        builder.HasOne(u => u.Address)
               .WithOne()
               .HasForeignKey<User>(u => u.AddressId);

        builder.HasOne(u => u.Image)
               .WithOne()
               .HasForeignKey<User>(u => u.AssetId);

        builder.HasMany(u => u.Transactions)
               .WithOne(t => t.User)
               .HasForeignKey(t => t.UserId);

        builder.HasMany(u => u.Reviews)
               .WithOne(r => r.User)
               .HasForeignKey(r => r.UserId);

        builder.HasOne(u => u.Store)
               .WithOne(s => s.Owner)
               .HasForeignKey<Store>(s => s.OwnerId)
               .OnDelete(DeleteBehavior.NoAction);
    }
}
