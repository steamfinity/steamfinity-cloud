using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountPasswordChangeRequest
{
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public string? NewPassword { get; init; }
}
