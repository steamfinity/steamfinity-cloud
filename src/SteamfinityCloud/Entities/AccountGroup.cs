namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a group of Steam accounts.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
public sealed class AccountGroup
{
    /// <summary>
    /// Gets or sets the unique identifier of the group.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the identifier of the user that is the owner of the group.
    /// </summary>
    public required Guid OwnerId { get; init; }

    /// <summary>
    /// Gets the user that is the owner of the group.
    /// </summary>
    public ApplicationUser Owner { get; } = null!;

    /// <summary>
    /// Gets or sets the name of the group.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the group.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the Steam launch parameters specified for all accounts in the group.
    /// </summary>
    /// <remarks>
    /// Group launch parameters have priority over global launch parameters but are overridden by per-account launch parameters.
    /// </remarks>
    public string? LaunchParameters { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the group was created.
    /// </summary>
    public DateTimeOffset CreationTime { get; init; } = DateTimeOffset.UtcNow;
}
