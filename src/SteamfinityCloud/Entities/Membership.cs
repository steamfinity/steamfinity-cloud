using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a membership of a user in a shared account library.
/// </summary>
[PrimaryKey(nameof(LibraryId), nameof(UserId))]
public sealed class Membership
{
    /// <summary>
    /// Gets or sets the identifier of the library that the user is a member of.
    /// </summary>
    public required Guid LibraryId { get; init; }

    /// <summary>
    /// Gets the library that the user is a member of.
    /// </summary>
    public Library Library { get; } = null!;

    /// <summary>
    /// Gets or sets the identifier of the user that is a member of the library.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets the user that is a member of the library.
    /// </summary>
    public ApplicationUser User { get; } = null!;

    /// <summary>
    /// Gets or sets the role of the library member.
    /// </summary>
    public MemberRole Role { get; set; }
}
