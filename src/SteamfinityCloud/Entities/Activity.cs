using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Entities;

public sealed class Activity
{
    public Guid Id { get; init; }

    public required ActivityType Type { get; init; }

    public Guid? InstigatorId { get; init; }

    public ApplicationUser? Instigator { get; }

    public Guid? TargetUserId { get; init; }

    public ApplicationUser? TargetUser { get; }

    public Guid? TargetLibraryId { get; init; }

    public Library? TargetLibrary { get; }

    public Guid? TargetAccountId { get; init; }

    public Account? TargetAccount { get; }

    public string? PreviousValue { get; init; }

    public string? NewValue { get; init; }

    public DateTimeOffset Time { get; init; } = DateTimeOffset.UtcNow;
}
