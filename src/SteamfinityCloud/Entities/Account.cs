﻿using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Entities;

[Index(nameof(SteamId))]
public sealed class Account
{
    public Guid Id { get; init; }

    public required Guid LibraryId { get; set; }

    public Library Library { get; } = null!;

    public required ulong SteamId { get; init; }

    public string? AccountName { get; set; }

    public string? OptimizedAccountName { get; set; }

    public string? Password { get; set; }

    public string? Alias { get; set; }

    public string? OptimizedAlias { get; set; }

    public SimpleColor Color { get; set; }

    public string? ProfileName { get; set; }

    public string? OptimizedProfileName { get; set; }

    public string? RealName { get; set; }

    public string? OptimizedRealName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? ProfileUrl { get; set; }

    public string? OptimizedProfileUrl { get; set; }

    public bool? IsProfileSetUp { get; set; }

    public bool? IsProfileVisible { get; set; }

    public bool? IsCommentingAllowed { get; set; }

    public AccountStatus? Status { get; set; }

    public ulong? CurrentGameId { get; set; }

    public string? CurrentGameName { get; set; }

    public string? OptimizedCurrentGameName { get; set; }

    public bool HasPrimeStatus { get; set; }

    public SkillGroup SkillGroup { get; set; }

    public bool? IsCommunityBanned { get; set; }

    public int? NumberOfVACBans { get; set; }

    public int? NumberOfGameBans { get; set; }

    public int? NumberOfDaysSinceLastBan { get; set; }

    public DateTimeOffset AdditionTime { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastUpdateTime { get; set; }

    public DateTimeOffset? LastEditTime { get; set; }

    public DateTimeOffset? LastRefreshTime { get; set; }

    public DateTimeOffset? CooldownExpirationTime { get; set; }

    public DateTimeOffset? CreationTime { get; set; }

    public DateTimeOffset? LastSignOutTime { get; set; }

    public string? LaunchParameters { get; set; }

    public string? OptimizedLaunchParameters { get; set; }

    public string? Notes { get; set; }

    public string? OptimizedNotes { get; set; }

    public ICollection<Hashtag> Hashtags { get; } = null!;

    public ICollection<AccountInteraction> Interactions { get; } = null!;

    public ICollection<Activity> Activities { get; } = null!;
}
