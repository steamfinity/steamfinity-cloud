using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record LibraryCreationRequest
{
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string Name { get; init; }

    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public string? Description { get; init; }
}
