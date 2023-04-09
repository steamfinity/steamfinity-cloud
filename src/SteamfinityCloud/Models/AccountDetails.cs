using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Models;

public sealed record AccountDetails
{
    public required Guid Id { get; init; }

    public required Guid LibraryId { get; init; }

    public required ulong SteamId { get; init; }

    public string? Alias { get; init; }

    public SimpleColor Color { get; init; }

    public string? ProfileName { get; set; }

    public string? RealName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? ProfileUrl { get; set; }

    public bool? IsProfileSetUp { get; set; }

    public bool? IsProfileVisible { get; set; }

    public bool? IsCommentingAllowed { get; set; }

    public AccountStatus? Status { get; set; }

    public ulong? CurrentGameId { get; set; }

    public string? CurrentGameName { get; set; }

    public bool? IsCommunityBanned { get; set; }

    public int? NumberOfVACBans { get; set; }

    public int? NumberOfGameBans { get; set; }

    public int? NumberOfDaysSinceLastBan { get; set; }

    public string? LaunchParameters { get; set; }

    public DateTimeOffset? CreationTime { get; set; }

    public DateTimeOffset? LastSignOutTime { get; set; }

    public required DateTimeOffset AdditionTime { get; init; }

    public DateTimeOffset? LastEditTimer { get; set; }

    public DateTimeOffset? LastUpdateTime { get; set; }

    public string? Notes { get; set; }
}
