using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a membership of a Steam account to a group. This is an internal database entity that should not be exposed to the client.
/// </summary>
/// <remarks>
/// One Steam account can be a member of multiple groups.
/// </remarks>
[PrimaryKey(nameof(GroupId), nameof(AccountId))]
public sealed class GroupMembership
{
    /// <summary>
    /// Gets or sets the identifier of the group that the account is a member of.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the group that the account is a member of.
    /// </summary>
    public AccountGroup Group { get; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the account that is a member of a group.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Gets the account that is a member of a group.
    /// </summary>
    public SteamAccount Account { get; } = null!;
}
