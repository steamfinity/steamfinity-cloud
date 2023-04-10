using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record IsFavoriteChangeRequest
{
    [Required]
    public required bool NewIsFavorite { get; init; }
}
