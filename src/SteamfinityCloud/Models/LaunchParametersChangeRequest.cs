using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record LaunchParametersChangeRequest
{
    [MaxLength(PropertyLengthConstraints.MaxLaunchParametersLength)]
    public string? NewLaunchParameters { get; init; }
}
