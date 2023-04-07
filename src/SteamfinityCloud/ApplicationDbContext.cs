using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

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
    public required DbSet<Account> Accounts { get; init; }

    /// <summary>
    /// Gets or sets the database set of all Steam account libraries.
    /// </summary>
    public required DbSet<Library> Libraries { get; init; }

    /// <summary>
    /// Gets or sets the database set of all library memberships.
    /// </summary>
    public required DbSet<Membership> Memberships { get; init; }

    /// <summary>
    /// Gets or sets the database set of all hashtags.
    /// </summary>
    public required DbSet<Hashtag> Hashtags { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Handle string <-> enum conversions:
        _ = builder.Entity<Account>().Property(a => a.Color).HasConversion(new EnumToStringConverter<SimpleColor>());
        _ = builder.Entity<Account>().Property(a => a.Status).HasConversion(new EnumToStringConverter<AccountStatus>());

        // Configure relationships:
        _ = builder.Entity<ApplicationUser>().HasMany(u => u.Memberships).WithOne(m => m.User).HasForeignKey(m => m.UserId);
        _ = builder.Entity<Library>().HasMany(l => l.Memberships).WithOne(m => m.Library).HasForeignKey(m => m.LibraryId);
        _ = builder.Entity<Library>().HasMany(l => l.Accounts).WithOne(a => a.Library).HasForeignKey(a => a.LibraryId);
    }
}
