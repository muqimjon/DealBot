namespace DealBot.Infrastructure.Persistence.EntityFramework;

using DealBot.Application.Common;
using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public required DbSet<User> Users { get; set; }
    public required DbSet<Card> Cards { get; set; }
    public required DbSet<Asset> Assets { get; set; }
    public required DbSet<Store> Stores { get; set; }
    public required DbSet<Contact> Contacts { get; set; }
    public required DbSet<Address> Addresses { get; set; }
    public required DbSet<Referral> Referrals { get; set; }
    public required DbSet<StoreReview> Reviews { get; set; }
    public required DbSet<MyMessage> MyMessages { get; set; }
    public required DbSet<Transaction> Transactions { get; set; }
    public required DbSet<CashbackSetting> CashbackSettings { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}