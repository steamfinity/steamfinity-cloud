using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record PasswordChangeRequest
{
    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string CurrentPassword { get; init; }

    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string NewPassword { get; init; }
}
