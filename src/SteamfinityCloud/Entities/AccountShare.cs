using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a Steam account share.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
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
    public Account Account { get; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the user whom the account is shared with.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets of sets the user whom the account is shared with.
    /// </summary>
    public ApplicationUser User { get; } = null!;

    /// <summary>
    /// Gets or sets a flag indicating whether the user can sign in to the account.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///        <item>This property has no effect when <c>IsAllowedToViewPassword</c> or <c>IsAllowedToEdit</c> are set to <see langword="true"/>.</item>
    ///        <item>The user may still be able to sign in if a group share allows this.</item>
    ///     </list>
    /// </remarks>
    /// <value><see langword="true"/> if the user is allowed to sign in, otherwise <see langword="false"/>.</value>
    public bool IsAllowedToSignIn { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the user can view the password of the account.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///        <item>This property has no effect when <c>IsAllowedToEdit</c> is set to <see langword="true"/>.</item>
    ///        <item>The user may still be able to view the password if a group share allows this.</item>
    ///     </list>
    /// </remarks>
    /// <value><see langword="true"/> if the user is allowed to view the password, otherwise <see langword="false"/>.</value>
    public bool IsAllowedToViewPassword { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the user can edit the account.
    /// </summary>
    /// <remarks>
    /// The user may still be able to edit the account if a group share allows this.
    /// </remarks>
    /// <value><see langword="true"/> if the user is allowed to edit the account, otherwise <see langword="false"/>.</value>
    public bool IsAllowedToEdit { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was shared.
    /// </summary>
    public DateTimeOffset TimeShared { get; init; } = DateTimeOffset.UtcNow;
}
