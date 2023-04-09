using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountAliasChangeRequest
{
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public string? NewAlias { get; init; }
}
