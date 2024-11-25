namespace DealBot.Infrastructure.Persistence.EntityFramework.EntityConfigurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CashbackSettingConfiguration : IEntityTypeConfiguration<CashbackSetting>
{
    public void Configure(EntityTypeBuilder<CashbackSetting> builder)
    {
        // Jadval nomi
        builder.ToTable("CashbackSettings");

        // Asosiy kalit
        builder.HasKey(cs => cs.Id);

        // Xususiyatlar konfiguratsiyasi
        ConfigureProperties(builder);
    }

    private static void ConfigureProperties(EntityTypeBuilder<CashbackSetting> builder)
    {
        builder.Property(cs => cs.Type)
            .IsRequired();

        builder.Property(cs => cs.Percentage)
            .HasColumnType("decimal(5, 2)")
            .IsRequired();
    }
}