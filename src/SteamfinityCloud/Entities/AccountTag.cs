using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a tag attached to an account.
/// </summary>
/// <remarks>
/// This is an internal database entity that should not be exposed to the client.
/// </remarks>
[PrimaryKey(nameof(AccountId), nameof(Name))]
[Index(nameof(Name))]
public sealed class AccountTag
{
    /// <summary>
    /// Gets or sets the identifier of the account that the tag is attached to.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Gets the account that the tag is attached to.
    /// </summary>
    public SteamAccount Account { get; } = null!;

    /// <summary>
    /// Gets or sets the name of the tag.
    /// </summary>
    public required string Name { get; init; }
}
