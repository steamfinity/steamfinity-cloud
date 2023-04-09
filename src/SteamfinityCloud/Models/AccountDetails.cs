using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Models;

public sealed record AccountDetails
{
    public required Guid Id { get; init; }

    public required Guid LibraryId { get; init; }

    public required ulong SteamId { get; init; }

    public string? Alias { get; init; }

    public bool IsFavorite { get; init; }

    public SimpleColor Color { get; init; }

    public string? ProfileName { get; init; }

    public string? RealName { get; init; }

    public string? AvatarUrl { get; init; }

    public string? ProfileUrl { get; init; }

    public bool? IsProfileSetUp { get; init; }

    public bool? IsProfileVisible { get; init; }

    public bool? IsCommentingAllowed { get; init; }

    public AccountStatus? Status { get; init; }

    public ulong? CurrentGameId { get; init; }

    public string? CurrentGameName { get; init; }

    public required SkillGroup SkillGroup { get; init; }

    public bool? IsCommunityBanned { get; init; }

    public int? NumberOfVACBans { get; init; }

    public int? NumberOfGameBans { get; init; }

    public int? NumberOfDaysSinceLastBan { get; init; }

    public required DateTimeOffset AdditionTime { get; init; }

    public DateTimeOffset? LastEditTime { get; set; }

    public DateTimeOffset? LastUpdateTime { get; set; }

    public DateTimeOffset? CreationTime { get; set; }

    public DateTimeOffset? LastSignOutTime { get; set; }

    public string? LaunchParameters { get; init; }

    public string? Notes { get; init; }
}
