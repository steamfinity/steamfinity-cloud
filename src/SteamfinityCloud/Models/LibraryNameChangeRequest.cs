using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record LibraryNameChangeRequest
{
    [Required]
    [MinLength(PropertyLengthConstraints.MinLibraryNameLength)]
    [MaxLength(PropertyLengthConstraints.MaxLibraryNameLength)]
    public required string NewName { get; init; }
}
