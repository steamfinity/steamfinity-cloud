using Steamfinity.Cloud.Enums;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record MemberAdditionRequest
{
    [Required]
    public required Guid UserId { get; init; }

    [Required]
    public required MemberRole Role { get; init; }
}
