using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountNotesChangeRequest
{
    [MaxLength(PropertyLengthConstraints.MaxNotesLength)]
    public string? NewNotes { get; init; }
}
