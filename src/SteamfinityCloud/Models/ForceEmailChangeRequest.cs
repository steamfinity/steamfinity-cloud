using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record ForceEmailChangeRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(PropertyLengthConstraints.MaxEmailLength)]
    public required string NewEmail { get; init; }
}
