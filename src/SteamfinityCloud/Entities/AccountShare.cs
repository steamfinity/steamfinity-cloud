using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a Steam account share. This is an internal database entity that should not be exposed to the client.
/// </summary>
[PrimaryKey(nameof(AccountId), nameof(UserId))]
public sealed class AccountShare
{
    /// <summary>
    /// Gets or sets the identifier of the shared account.
    /// </summary>
    public required Guid AccountId { get; init; }

    /// <summary>
    /// Gets the shared account.
    /// </summary>
    public SteamAccount Account { get; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the user whom the account is shared with.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets of sets the user whom the account is shared with.
    /// </summary>
    public ApplicationUser User { get; } = null!;

    /// <summary>
    /// Gets or sets a flag indicating whether the user is allowed to use the account.
    /// </summary>
    /// <remarks>
    /// The value of this property is ignored (always true) when <see cref="IsAllowedToViewPassword"/> is set to true.
    /// </remarks>
    /// <value>True if the user is allowed to sign in, otherwise false.</value>
    public bool IsAllowedToSignIn { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the user is allowed to view the password.
    /// </summary>
    /// <remarks>
    /// The value of this property is ignored (always true) when <see cref="IsAllowedToEdit"/> is set to true.
    /// </remarks>
    /// <value>True if the user is allowed to view the password, otherwise false.</value>
    public bool IsAllowedToViewPassword { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the user is allowed to edit the information.
    /// </summary>
    /// <value>True if the user is allowed to edit, otherwise false.</value>
    public bool IsAllowedToEdit { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was shared.
    /// </summary>
    public DateTimeOffset TimeShared { get; init; } = DateTimeOffset.UtcNow;
}
