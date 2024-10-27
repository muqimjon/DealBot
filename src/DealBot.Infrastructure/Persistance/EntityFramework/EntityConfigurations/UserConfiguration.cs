namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
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

        builder.Property(u => u.Role)
               .IsRequired();

        builder.Property(u => u.TelegramId)
               .IsRequired();

        builder.Property(u => u.ChatId)
               .IsRequired();

        builder.Property(u => u.State)
               .IsRequired();

        builder.Property(u => u.IsActive)
               .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(u => u.CustomerTransactions)
               .WithOne(t => t.Customer)
               .HasForeignKey(t => t.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.SellerTransactions)
               .WithOne(t => t.Seller)
               .HasForeignKey(t => t.SellerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
