using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record HasPrimeStatusChangeRequest
{
    [Required]
    public required bool NewHasPrimeStatus { get; init; }
}
