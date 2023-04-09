using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountHasPrimeStatusChangeRequest
{
    [Required]
    public required bool NewHasPrimeStatus { get; init; }
}
