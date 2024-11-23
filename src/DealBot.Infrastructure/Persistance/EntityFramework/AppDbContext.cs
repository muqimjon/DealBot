namespace DealBot.Infrastructure.Persistance.EntityFramework;

using DealBot.Application.Common;
using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    public DbSet<StoreReview> Reviews { get; set; }
    public DbSet<MyMessage> MyMessages { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<CashbackSetting> CashbackSettings { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}