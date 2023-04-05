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
}
