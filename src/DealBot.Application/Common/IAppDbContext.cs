namespace DealBot.Application.Common;

using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Asset> Assets { get; }
    DbSet<Referral> Referrals { get; }
    DbSet<Store> Stores { get; }
    DbSet<StoreReview> Reviews { get; }
    DbSet<Transaction> Transactions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}