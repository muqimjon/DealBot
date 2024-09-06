namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> builder)
    {
        // Table name
        builder.ToTable("Referrals");

        // Primary key
        builder.HasKey(r => r.Id);

        // Properties
        ConfigureProperties(builder);

        // Relationships
        ConfigureRelationships(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Referral> builder)
    {
        builder.Property(r => r.ReferralCode)
                       .IsRequired()
                       .HasMaxLength(50);

        builder.Property(r => r.IsRewarded)
               .IsRequired();

        builder.Property(r => r.RewardAmount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<Referral> builder)
    {
        builder.HasOne(r => r.Referrer)
               .WithMany(u => u.ReferralsInitiated)
               .HasForeignKey(r => r.ReferrerId);

        builder.HasOne(r => r.Referred)
               .WithOne(u => u.ReferredBy)
               .HasForeignKey<Referral>(r => r.ReferredId);
    }
}