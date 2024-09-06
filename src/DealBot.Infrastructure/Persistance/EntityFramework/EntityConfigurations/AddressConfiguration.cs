namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

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
        builder.Property(a => a.Latitude)
                       .IsRequired();

        builder.Property(a => a.Longitude)
               .IsRequired();
    }
}





