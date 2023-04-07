﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

        builder.Entity<Account>().Property(a => a.Color).HasConversion(new EnumToStringConverter<SimpleColor>());
        builder.Entity<Account>().Property(a => a.Status).HasConversion(new EnumToStringConverter<AccountStatus>());
        builder.Entity<Account>().HasMany(a => a.Hashtags).WithOne(t => t.Account).HasForeignKey(t => t.AccountId);
    }
}
