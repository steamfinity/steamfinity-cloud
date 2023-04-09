using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record LibraryDescriptionChangeRequest
{
    [Required]
    [MaxLength(PropertyLengthConstraints.MaxLibraryDescriptionLength)]
    public required string NewDescription { get; init; }
}
