using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountAdditionRequest
{
    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string SteamId { get; init; }
}
