using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountTransferRequest
{
    [Required]
    public Guid NewLibraryId { get; init; }
}
