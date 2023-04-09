using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record PasswordChangeRequest
{
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string CurrentPassword { get; init; }

    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewPassword { get; init; }
}
