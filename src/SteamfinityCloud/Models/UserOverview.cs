namespace Steamfinity.Cloud.Models;

public sealed record UserOverview
{
    public required Guid Id { get; init; }

    public string? UserName { get; init; }
}
