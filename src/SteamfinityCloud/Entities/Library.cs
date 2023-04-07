namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a library of Steam accounts.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
public sealed class Library
{
    /// <summary>
    /// Gets or sets the unique identifier of the library.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the display name of the library.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the library.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the library was created.
    /// </summary>
    public DateTimeOffset CreationTime { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the collection of all memberships in this library.
    /// </summary>
    public ICollection<Membership> Memberships { get; } = null!;

    /// <summary>
    /// Gets the collection of all Steam accounts that belong to the library.
    /// </summary>
    public ICollection<Account> Accounts { get; } = null!;
}
