namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        // Jadval nomi
        builder.ToTable("Cards");

        // Asosiy kalit
        builder.HasKey(c => c.Id);

        // Xususiyatlar konfiguratsiyasi
        ConfigureProperties(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<Card> builder)
    {
        builder.Property(c => c.Ballance)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();
    }
}
