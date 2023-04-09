namespace Steamfinity.Cloud.Models;

public sealed record AuthenticationTokenDetails
{
    public required string Token { get; init; }

    public required DateTimeOffset ExpirationTime { get; init; }

    public required IEnumerable<string> Roles { get; init; }
}
