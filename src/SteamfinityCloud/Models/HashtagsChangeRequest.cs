using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record HashtagsChangeRequest
{
    [Required]
    public required IEnumerable<string> NewHashtags { get; init; }
}
