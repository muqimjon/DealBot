namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

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
                       .HasColumnType("decimal(18,2)")
                       .IsRequired();

        builder.Property(t => t.Status)
               .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasOne(t => t.User)
                       .WithMany(u => u.Transactions)
                       .HasForeignKey(t => t.UserId);

        builder.HasOne(t => t.Store)
               .WithMany(s => s.Transactions)
               .HasForeignKey(t => t.StoreId);
    }
}