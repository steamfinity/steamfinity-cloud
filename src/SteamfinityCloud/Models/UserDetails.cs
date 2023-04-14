namespace Steamfinity.Cloud.Models;

public sealed record UserDetails
{
    public required Guid Id { get; init; }

    public string? UserName { get; init; }

    public required DateTimeOffset SignUpTime { get; init; }

    public required IEnumerable<string> Roles { get; init; }
}
