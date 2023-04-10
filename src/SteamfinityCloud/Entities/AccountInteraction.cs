using Microsoft.EntityFrameworkCore;

namespace Steamfinity.Cloud.Entities;

[PrimaryKey(nameof(AccountId), nameof(UserId))]
public sealed class AccountInteraction
{
    public required Guid AccountId { get; init; }

    public Account Account { get; } = null!;

    public required Guid UserId { get; init; }

    public ApplicationUser User { get; } = null!;

    public bool IsFavorite { get; set; }
}
