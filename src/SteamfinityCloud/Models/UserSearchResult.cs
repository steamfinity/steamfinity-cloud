namespace Steamfinity.Cloud.Models;

public sealed record UserSearchResult
{
    public required Guid UserId { get; init; }
}
