using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a Steam account. This is an internal database entity that should not be exposed to the client.
/// </summary>
[Index(nameof(SteamId))]
public sealed class SteamAccount
{
    /// <summary>
    /// Gets or sets the unique identifier of the account.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user that is the owner of the account.
    /// </summary>  
    public required Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the user that is the owner of the account.
    /// </summary>
    public ApplicationUser Owner { get; } = null!;

    /// <summary>
    /// Gets or sets the Steam ID of the account.
    /// </summary>
    public required ulong SteamId { get; init; }

    /// <summary>
    /// Gets or sets the account name (login) of the account.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>Providing the account name is not required, but highly recommended.</item>
    ///         <item>This value should be protected using end-to-end encryption.</item>
    ///     </list>
    /// </remarks>
    public string? AccountName { get; set; }

    /// <summary>
    /// Gets or sets the password of the account.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>Providing the password is optional.</item>
    ///         <item>This value should be protected using end-to-end encryption.</item>
    ///     </list>
    /// </remarks>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the profile name (display name) of the account.
    /// </summary>
    public string? ProfileName { get; set; }

    /// <summary>
    /// Gets or sets the real name of the account.
    /// </summary>
    public string? RealName { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL of the account.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the profile URL of the account.
    /// </summary>
    public string? ProfileUrl { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating if the profile of the account has been set up.
    /// </summary>
    /// <value>True if the profile has been set up, otherwise false.</value>
    public bool? IsProfileSetUp { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating if the profile of the account is visible.
    /// </summary>
    /// <value>True if the profile is visible, otherwise false.</value>
    public bool? IsProfileVisible { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the account is accepting new comments.
    /// </summary>
    /// <value>True if the account has comments enabled, otherwise false.</value>
    public bool? IsCommentingAllowed { get; set; }

    /// <summary>
    /// Gets or sets the status of the account.
    /// </summary>
    public AccountStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the game that the account is currently playing.
    /// </summary>
    public ulong? CurrentGameId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the game that the account is currently playing.
    /// </summary>
    public string? CurrentGameName { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the account is banned from the Steam community.
    /// </summary>
    /// <value>True if the account is community banned, otherwise false.</value>
    public bool? IsCommunityBanned { get; set; }

    /// <summary>
    /// Gets or sets the total number of VAC bans that the account currently has.
    /// </summary>
    public int? NumberOfVACBans { get; set; }

    /// <summary>
    /// Gets or sets the total number of game bans that the account currently has.
    /// </summary>
    public int? NumberOfGameBans { get; set; }

    /// <summary>
    /// Gets or sets the number of days that have passed since the last ban was issued.
    /// </summary>
    public int? NumberOfDaysSinceLastBan { get; set; }

    /// <summary>
    /// Gets or sets the Steam launch parameters for the account.
    /// </summary>
    public string? LaunchParameters { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was created.
    /// </summary>
    public DateTimeOffset? SignUpTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was last accessed.
    /// </summary>
    public DateTimeOffset? LastSignOutTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was added to Steamfinity.
    /// </summary>
    public DateTimeOffset AdditionTime { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the account was last manually edited.
    /// </summary>
    public DateTimeOffset? LastEditTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was last updated with the data obtained from the Steam API.
    /// </summary>
    public DateTimeOffset? LastUpdateTime { get; set; }

    /// <summary>
    /// Gets or sets the user's notes for this account.
    /// </summary>
    public string? Notes { get; set; }
}
