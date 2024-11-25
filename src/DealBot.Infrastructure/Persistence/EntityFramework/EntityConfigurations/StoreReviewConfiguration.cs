namespace DealBot.Infrastructure.Persistence.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StoreReviewConfiguration : IEntityTypeConfiguration<StoreReview>
{
    public void Configure(EntityTypeBuilder<StoreReview> builder)
    {
        // Table name
        builder.ToTable("StoreReviews");

        // Primary key
        builder.HasKey(sr => sr.Id);

        // Properties
        ConfigureProperties(builder);

        // Relationships
        ConfigureRelationships(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<StoreReview> builder)
    {
        builder.Property(sr => sr.Rating)
                       .IsRequired();

        builder.Property(sr => sr.Comment)
               .HasMaxLength(500);
    }

    private static void ConfigureRelationships(EntityTypeBuilder<StoreReview> builder)
    {
        builder.HasOne(sr => sr.Store)
                       .WithMany(s => s.Reviews)
                       .HasForeignKey(sr => sr.StoreId);
    }
}