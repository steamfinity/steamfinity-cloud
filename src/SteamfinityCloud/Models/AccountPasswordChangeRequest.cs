using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountPasswordChangeRequest
{
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public string? NewPassword { get; init; }
}
