using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Entities;

[PrimaryKey(nameof(LibraryId), nameof(UserId))]
public sealed class Membership
{
    public required Guid LibraryId { get; init; }

    public Library Library { get; } = null!;

    public required Guid UserId { get; init; }

    public ApplicationUser User { get; } = null!;

    public MemberRole Role { get; set; }
}
