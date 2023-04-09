namespace Steamfinity.Cloud.Models;

public sealed record AccountOverview
{
    public required Guid Id { get; init; }

    public string? ProfileName { get; init; }

    public string? AvatarUrl { get; init; }
}
