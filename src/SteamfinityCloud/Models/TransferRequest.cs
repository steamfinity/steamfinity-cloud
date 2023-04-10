using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record TransferRequest
{
    [Required]
    public Guid NewLibraryId { get; init; }
}
