using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

[PrimaryKey(nameof(AccountId), nameof(Name))]
[Index(nameof(Name))]
public sealed class Hashtag
{
    public required Guid AccountId { get; init; }

    public Account Account { get; } = null!;

    public required string Name { get; init; }
}
