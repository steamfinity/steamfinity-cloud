using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record ForceEmailChangeRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewEmail { get; init; }
}
