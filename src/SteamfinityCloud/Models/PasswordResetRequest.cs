using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record PasswordResetRequest
{
    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string NewPassword { get; init; }
}
