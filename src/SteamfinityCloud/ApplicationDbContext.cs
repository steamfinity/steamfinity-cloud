using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud;

/// <summary>
/// Represents a database context in the Steamfinity Cloud.
/// </summary>
public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the database context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    /// <summary>
    /// Gets or sets the database set of all Steam accounts.
    /// </summary>
    public required DbSet<SteamAccount> Accounts { get; init; }

    /// <summary>
    /// Gets or sets the database set of all Steam account groups.
    /// </summary>
    public required DbSet<AccountGroup> Groups { get; init; }

    /// <summary>
    /// Gets or sets the database set of all Steam account group memberships.
    /// </summary>
    public required DbSet<GroupMembership> Memberships { get; init; }

    /// <summary>
    /// Gets or sets the database set of all account shares.
    /// </summary>
    public required DbSet<AccountShare> AccountShares { get; init; }

    /// <summary>
    /// Gets or sets the database set of all group shares.
    /// </summary>
    public required DbSet<GroupShare> GroupShares { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().HasMany(u => u.OwnedAccounts).WithOne(a => a.Owner).HasForeignKey(a => a.OwnerId);
        builder.Entity<ApplicationUser>().HasMany(u => u.OwnedGroups).WithOne(g => g.Owner).HasForeignKey(g => g.OwnerId);
        builder.Entity<ApplicationUser>().HasMany(u => u.AccountShares).WithOne(s => s.User).HasForeignKey(s => s.UserId);
        builder.Entity<ApplicationUser>().HasMany(u => u.GroupShares).WithOne(s => s.User).HasForeignKey(s => s.UserId);

        builder.Entity<SteamAccount>().HasMany(a => a.Memberships).WithOne(m => m.Account).HasForeignKey(m => m.AccountId);
        builder.Entity<SteamAccount>().HasMany(a => a.Shares).WithOne(s => s.Account).HasForeignKey(s => s.AccountId);

        builder.Entity<AccountGroup>().HasMany(g => g.Memberships).WithOne(m => m.Group).HasForeignKey(m => m.GroupId);
        builder.Entity<AccountGroup>().HasMany(g => g.Shares).WithOne(s => s.Group).HasForeignKey(s => s.GroupId);
    }
}
