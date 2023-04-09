using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record UserDeletionRequest
{
    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string Password { get; init; }
}
