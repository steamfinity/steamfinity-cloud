using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Entities;

[Index(nameof(SteamId))]
public sealed class Account
{
    public Guid Id { get; init; }

    public required Guid LibraryId { get; init; }

    public Library Library { get; } = null!;

    public required ulong SteamId { get; init; }

    public string? AccountName { get; set; }

    public string? Password { get; set; }

    public string? Alias { get; set; }

    public bool IsFavorite { get; set; }

    public SimpleColor Color { get; set; }

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

    public DateTimeOffset AdditionTime { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastEditTime { get; set; }

    public DateTimeOffset? LastUpdateTime { get; set; }

    public DateTimeOffset? CreationTime { get; set; }

    public DateTimeOffset? LastSignOutTime { get; set; }

    public string? LaunchParameters { get; set; }

    public string? Notes { get; set; }

    public ICollection<Hashtag> Hashtags { get; } = null!;
}
