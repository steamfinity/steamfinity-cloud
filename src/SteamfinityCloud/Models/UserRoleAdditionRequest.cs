using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record UserRoleAdditionRequest
{
    [Required]
    public required string RoleName { get; init; }
}
