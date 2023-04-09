using Steamfinity.Cloud.Enums;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record MemberRoleChangeRequest
{
    [Required]
    public required MemberRole NewRole { get; init; }
}
