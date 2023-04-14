namespace Steamfinity.Cloud.Models;

public sealed record CooldownExpirationTimeChangeRequest
{
    public required DateTimeOffset NewCooldownExpirationTime { get; init; }
}
