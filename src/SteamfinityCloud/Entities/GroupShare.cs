using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents an account group share.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
[PrimaryKey(nameof(GroupId), nameof(UserId))]
public sealed class GroupShare
{
    /// <summary>
    /// Gets or sets the identifier of the shared group.
    /// </summary>
    public required Guid GroupId { get; init; }

    /// <summary>
    /// Gets the shared group.
    /// </summary>
    public AccountGroup Group { get; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the user whom the group is shared with.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the user whom the group is shared with.
    /// </summary>
    public ApplicationUser User { get; } = null!;

    /// <summary>
    /// Gets or sets a flag indicating whether the user can sign in to all accounts in the group.
    /// </summary>
    /// <remarks>
    /// The user may still be able to sign in if another group or an individual share allows this.
    /// </remarks>
    /// <value><see langword="true"/> if the user is allowed to sign in, otherwise <see langword="false"/>.</value>
    public bool IsAllowedToSignIn { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the user can view the passwords of all accounts in the group.
    /// </summary>
    /// <remarks>
    /// The user may still be able to view the password if another group or an individual share allows this.
    /// </remarks>
    /// <value><see langword="true"/> if the user is allowed to view the passwords, otherwise <see langword="false"/>.</value>
    public bool IsAllowedToViewPasswords { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the user can edit the group and all its accounts.
    /// </summary>
    /// <remarks>
    /// The user may still be able to edit an account if another group or an individual share allows this.
    /// </remarks>
    /// <value><see langword="true"/> if the user is allowed to edit the group and its accounts, otherwise <see langword="false"/>.</value>
    public bool IsAllowedToEdit { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the group was shared.
    /// </summary>
    public DateTimeOffset TimeShared { get; init; } = DateTimeOffset.UtcNow;
}
