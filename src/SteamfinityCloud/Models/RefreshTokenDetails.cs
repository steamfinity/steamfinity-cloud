namespace Steamfinity.Cloud.Models;

public sealed record RefreshTokenDetails
{
    public required Guid UserId { get; init; }

    public required string RefreshToken { get; init; }
}
