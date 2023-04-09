namespace Steamfinity.Cloud.Models;

public sealed record AccountOverview
{
    public required Guid Id { get; init; }

    public required string? ProfileName { get; init; }

    public required string? AvatarUrl { get; init; }
}
