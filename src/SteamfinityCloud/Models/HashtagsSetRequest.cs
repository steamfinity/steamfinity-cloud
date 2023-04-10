﻿using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record HashtagsSetRequest
{
    [Required]
    public required IEnumerable<string> NewHashtags { get; init; }
}