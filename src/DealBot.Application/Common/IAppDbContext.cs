namespace DealBot.Application.Common;

using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Card> Cards { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Asset> Assets { get; }
    DbSet<Referral> Referrals { get; }
    DbSet<Store> Stores { get; }
    DbSet<StoreReview> Reviews { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<CashbackSetting> CashbackSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}