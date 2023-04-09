using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountNotesChangeRequest
{
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public string? NewNotes { get; init; }
}
