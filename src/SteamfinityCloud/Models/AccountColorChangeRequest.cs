using Steamfinity.Cloud.Enums;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountColorChangeRequest
{
    [Required]
    public required SimpleColor NewColor { get; init; }
}
