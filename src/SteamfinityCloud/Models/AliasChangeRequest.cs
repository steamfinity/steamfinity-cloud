using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record AliasChangeRequest
{
    [MaxLength(PropertyLengthConstraints.MaxAliasLength)]
    public string? NewAlias { get; init; }
}
