using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record UserSuspensionChangeRequest
{
    [Required]
    public required bool NewIsSuspended { get; init; }
}
