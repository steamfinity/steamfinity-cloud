using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AccountAliasChangeRequest
{
    [MaxLength(PropertyLengthConstraints.MaxAliasLength)]
    public string? NewAlias { get; init; }
}
