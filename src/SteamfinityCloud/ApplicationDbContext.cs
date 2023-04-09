using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public required DbSet<Account> Accounts { get; init; }

    public required DbSet<Library> Libraries { get; init; }

    public required DbSet<Membership> Memberships { get; init; }

    public required DbSet<Hashtag> Hashtags { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure string <-> enum conversions:
        _ = builder.Entity<Account>().Property(a => a.Color).HasConversion(new EnumToStringConverter<SimpleColor>());
        _ = builder.Entity<Account>().Property(a => a.Status).HasConversion(new EnumToStringConverter<AccountStatus>());

        // Configure relationships:
        _ = builder.Entity<ApplicationUser>().HasMany(u => u.Memberships).WithOne(m => m.User).HasForeignKey(m => m.UserId);
        _ = builder.Entity<Library>().HasMany(l => l.Memberships).WithOne(m => m.Library).HasForeignKey(m => m.LibraryId);
        _ = builder.Entity<Library>().HasMany(l => l.Accounts).WithOne(a => a.Library).HasForeignKey(a => a.LibraryId);
    }
}
