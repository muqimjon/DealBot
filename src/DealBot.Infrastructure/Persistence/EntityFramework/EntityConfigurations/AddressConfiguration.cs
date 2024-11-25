namespace DealBot.Infrastructure.Persistence.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        // Table name
        builder.ToTable("Addresses");

        // Primary key
        builder.HasKey(a => a.Id);

        // Properties
        ConfigureProperties(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Address> builder)
    {
        builder.Property(a => a.Country)
            .IsRequired(false);

        builder.Property(a => a.CountryCode)
            .IsRequired(false);

        builder.Property(a => a.Region)
            .IsRequired(false);

        builder.Property(a => a.District)
            .IsRequired(false);

        builder.Property(a => a.City)
            .IsRequired(false);

        builder.Property(a => a.Street)
            .IsRequired(false);

        builder.Property(a => a.HouseNumber)
            .IsRequired(false);
    }
}





