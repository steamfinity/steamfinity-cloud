using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record EmailChangeRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewEmail { get; init; }

    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string Password { get; init; }
}
