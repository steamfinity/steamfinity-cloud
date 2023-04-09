using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Models;

public sealed record AccountDetails
{
    public required Guid Id { get; init; }

    public required Guid LibraryId { get; init; }

    public required ulong SteamId { get; init; }

    public required string? Alias { get; init; }

    public required bool IsFavorite { get; init; }

    public required SimpleColor Color { get; init; }

    public required string? ProfileName { get; init; }

    public required string? RealName { get; init; }

    public required string? AvatarUrl { get; init; }

    public required string? ProfileUrl { get; init; }

    public required bool? IsProfileSetUp { get; init; }

    public required bool? IsProfileVisible { get; init; }

    public required bool? IsCommentingAllowed { get; init; }

    public required AccountStatus? Status { get; init; }

    public required ulong? CurrentGameId { get; init; }

    public required string? CurrentGameName { get; init; }

    public required bool HasPrimeStatus { get; init; }

    public required SkillGroup SkillGroup { get; init; }

    public required bool? IsCommunityBanned { get; init; }

    public required int? NumberOfVACBans { get; init; }

    public required int? NumberOfGameBans { get; init; }

    public required int? NumberOfDaysSinceLastBan { get; init; }

    public required DateTimeOffset AdditionTime { get; init; }

    public required DateTimeOffset? LastEditTime { get; set; }

    public required DateTimeOffset? LastUpdateTime { get; init; }

    public required DateTimeOffset? CompetitiveCooldownExpirationTime { get; init; }

    public required DateTimeOffset? CreationTime { get; init; }

    public required DateTimeOffset? LastSignOutTime { get; init; }

    public required string? LaunchParameters { get; init; }

    public required string? Notes { get; init; }

    public required IEnumerable<string> Hashtags { get; init; }
}
