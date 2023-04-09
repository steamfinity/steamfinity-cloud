namespace Steamfinity.Cloud.Entities;

public sealed class Library
{
    public Guid Id { get; init; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreationTime { get; init; } = DateTimeOffset.UtcNow;

    public ICollection<Membership> Memberships { get; } = null!;

    public ICollection<Account> Accounts { get; } = null!;
}
