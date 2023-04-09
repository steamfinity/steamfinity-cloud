using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountNameChangeRequest
{
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public string? NewAccountName { get; init; }
}
