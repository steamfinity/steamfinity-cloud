using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents an account's membership in a group.
/// </summary>
/// <remarks>
///     <list type="bullet">
///         <item>One account can be a member of multiple groups.</item>
///         <item>This is an internal database entity and should not be exposed to the client.</item>
///     </list>
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
    public Account Account { get; } = null!;
}
