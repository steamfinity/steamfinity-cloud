using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Steamfinity.Cloud.Entities;

[PrimaryKey(nameof(AccountId), nameof(Name))]
[Index(nameof(Name))]
public sealed class Hashtag
{
    [SetsRequiredMembers]
    public Hashtag(Guid accountId, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        AccountId = accountId;
        Name = name;
    }

    public required Guid AccountId { get; init; }

    public Account Account { get; } = null!;

    public required string Name { get; init; }
}
