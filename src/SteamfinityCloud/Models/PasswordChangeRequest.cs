using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record PasswordChangeRequest
{
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public string? CurrentPassword { get; init; }

    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string NewPassword { get; init; }
}
