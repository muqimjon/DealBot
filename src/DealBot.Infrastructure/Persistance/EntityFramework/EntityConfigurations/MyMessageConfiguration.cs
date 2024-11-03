namespace DealBot.Infrastructure.Persistance.EntityFramework.EntityConfigurations;

using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MyMessageConfiguration : IEntityTypeConfiguration<MyMessage>
{
    public void Configure(EntityTypeBuilder<MyMessage> builder)
    {
        // Jadval nomi
        builder.ToTable("Messages");

        // Asosiy kalit
        builder.HasKey(m => m.Id);

        // Xususiyatlar konfiguratsiyasi
        ConfigureProperties(builder);

        // Foreign Key konfiguratsiyasi
        ConfigureRelationships(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<MyMessage> builder)
    {
        builder.Property(m => m.Title)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(m => m.Content)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(m => m.Type)
            .IsRequired();

        builder.Property(m => m.Gender)
            .IsRequired();

        builder.Property(m => m.Role)
            .IsRequired();

        builder.Property(m => m.Status)
            .IsRequired();
    }

    private static void ConfigureRelationships(EntityTypeBuilder<MyMessage> builder)
    {
        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
