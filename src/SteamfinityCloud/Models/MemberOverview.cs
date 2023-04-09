using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Models;

public sealed record MemberOverview
{
    public required Guid Id { get; init; }

    public string? UserName { get; init; }

    public required MemberRole Role { get; init; }
}
