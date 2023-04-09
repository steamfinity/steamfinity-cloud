using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record EmailChangeRequest
{
    [Required]
    [EmailAddress]
    [MinLength(PropertyLengthConstraints.MinEmailLength)]
    [MaxLength(PropertyLengthConstraints.MaxEmailLength)]
    public required string NewEmail { get; init; }

    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string Password { get; init; }
}
