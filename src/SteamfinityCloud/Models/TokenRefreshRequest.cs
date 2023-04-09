using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record TokenRefreshRequest
{
    [Required]
    public required Guid UserId { get; init; }

    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string RefreshToken { get; init; }
}
