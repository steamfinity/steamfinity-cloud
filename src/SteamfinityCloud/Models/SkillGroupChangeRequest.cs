using Steamfinity.Cloud.Enums;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record SkillGroupChangeRequest
{
    [Required]
    public required SkillGroup NewSkillGroup { get; init; }
}
