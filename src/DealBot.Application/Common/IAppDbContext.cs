namespace DealBot.Application.Common;

using DealBot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Card> Cards { get; }
    DbSet<Asset> Assets { get; }
    DbSet<Store> Stores { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Referral> Referrals { get; }
    DbSet<StoreReview> Reviews { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<MyMessage> MyMessages { get; set; }
    DbSet<CashbackSetting> CashbackSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}