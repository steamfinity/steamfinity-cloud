using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Models;

public sealed record LibraryDetails
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string? Description { get; init; }

    public required MemberRole Role { get; init; }

    public required DateTimeOffset CreationTime { get; init; }
}
