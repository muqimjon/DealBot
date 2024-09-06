namespace DealBot.Infrastructure.Persistance.EntityFramework;

using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<StoreReview> Reviews { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}