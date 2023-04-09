using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record LaunchParametersChangeRequest
{
    [Required]
    [MaxLength(PropertyLengthConstraints.MaxLaunchParametersLength)]
    public required string NewLaunchParameters { get; init; }
}
