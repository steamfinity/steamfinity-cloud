using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountIsFavoriteChangeRequest
{
    [Required]
    public required bool NewIsFavorite { get; init; }
}
