namespace DealBot.Infrastructure.Persistence.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        // Table name
        builder.ToTable("Contacts");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        ConfigureProperties(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Contact> builder)
    {
        builder.Property(c => c.Phone)
                       .HasMaxLength(20);

        builder.Property(c => c.Email)
               .HasMaxLength(76);
    }
}