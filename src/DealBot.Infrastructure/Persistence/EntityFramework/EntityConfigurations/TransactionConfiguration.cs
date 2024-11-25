namespace DealBot.Infrastructure.Persistence.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Table name
        builder.ToTable("Transactions");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        ConfigureProperties(builder);

        // Relationships
        ConfigureRelationships(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Status)
            .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasOne(t => t.Customer)
            .WithMany(u => u.CustomerTransactions)
            .HasForeignKey(t => t.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Seller)
            .WithMany(u => u.SellerTransactions)
            .HasForeignKey(t => t.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
