using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a Steam account.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
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
    /// Gets or sets the password for the account.
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
    /// Gets or sets the real name specified in the about section.
    /// </summary>
    /// <remarks>
    /// The value of this property may be unavailable if the account is private or friends only.
    /// </remarks>
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
    /// Gets or sets a flag indicating whether the profile of the account has been set up.
    /// </summary>
    /// <value><see langword="true"/> if the profile has been set up, otherwise <see langword="false"/>.</value>
    public bool? IsProfileSetUp { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the profile of the account is visible to Steamfinity.
    /// </summary>
    /// <value><see langword="true"/> if the profile is visible, otherwise <see langword="false"/>.</value>
    public bool? IsProfileVisible { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the account is accepting new comments.
    /// </summary>
    /// <value><see langword="true"/> if the account has comments enabled, otherwise <see langword="false"/>.</value>
    public bool? IsCommentingAllowed { get; set; }

    /// <summary>
    /// Gets or sets the activity status of the account.
    /// </summary>
    /// <remarks>
    /// The value of this property may be inaccurate or unavailable if the account is private or friends only.
    /// </remarks>
    public AccountStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the game that the account is currently playing.
    /// </summary>
    /// <remarks>
    /// The value of this property may be unavailable if the account is private or friends only.
    /// </remarks>
    public ulong? CurrentGameId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the game that the account is currently playing.
    /// </summary>
    /// <remarks>
    /// The value of this property may be unavailable if the account is private or friends only.
    /// </remarks>
    public string? CurrentGameName { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating whether the account is banned from the Steam community.
    /// </summary>
    /// <value><see langword="true"/> if the account is community banned, otherwise <see langword="false"/>.</value>
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
    /// Gets or sets the number of days that have passed since the last ban was imposed.
    /// </summary>
    public int? NumberOfDaysSinceLastBan { get; set; }

    /// <summary>
    /// Gets or sets the Steam launch parameters specified for the account.
    /// </summary>
    /// <remarks>
    /// Account launch parameters have priority over global and group launch parameters.
    /// </remarks>
    public string? LaunchParameters { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was created.
    /// </summary>
    /// <remarks>
    /// The value of this property may be unavailable if the account is private or friends only.
    /// </remarks>
    public DateTimeOffset? TimeCreated { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was last online.
    /// </summary>
    /// <remarks>
    /// The value of this property may be unavailable if the account is private or friends only.
    /// </remarks>
    public DateTimeOffset? TimeSignedOut { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was added to Steamfinity.
    /// </summary>
    public DateTimeOffset TimeAdded { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the account information was last manually edited.
    /// </summary>
    public DateTimeOffset? TimeEdited { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the account was last updated with the data obtained from the Steam API.
    /// </summary>
    public DateTimeOffset? TimeUpdated { get; set; }

    /// <summary>
    /// Gets or sets the user's notes for this account.
    /// </summary>
    public string? Notes { get; set; }
}
