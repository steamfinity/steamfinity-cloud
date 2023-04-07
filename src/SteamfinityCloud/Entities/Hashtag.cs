using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a hashtag added to an account for easier organization.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
[PrimaryKey(nameof(AccountId), nameof(Name))]
[Index(nameof(Name))]
public sealed class Hashtag
{
    /// <summary>
    /// Gets or sets the identifier of the account that the hashtag is added to.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Gets the account that the hashtag is added to.
    /// </summary>
    public Account Account { get; } = null!;

    /// <summary>
    /// Gets or sets the name of the hashtag.
    /// </summary>
    public required string Name { get; init; }
}
