using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record LibraryCreationRequest
{
    [Required]
    [MinLength(PropertyLengthConstraints.MinLibraryNameLength)]
    [MaxLength(PropertyLengthConstraints.MaxLibraryNameLength)]
    public required string Name { get; init; }

    [MaxLength(PropertyLengthConstraints.MaxLibraryDescriptionLength)]
    public string? Description { get; init; }
}
