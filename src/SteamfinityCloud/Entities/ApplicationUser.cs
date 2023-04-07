﻿using Microsoft.AspNetCore.Identity;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a user of the Steamfinity Cloud.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Gets or sets a value indicating whether the user is currently suspended. 
    /// A suspended user is prevented from authenticating and using the application.
    /// </summary>
    public bool IsSuspended { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the user created their Steamfinity Cloud account.
    /// </summary>
    public DateTimeOffset SignUpTime { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the user last signed in to the Steamfinity Cloud.
    /// This value is updated with every successful authentication token renewal.
    /// </summary>
    public DateTimeOffset? LastSignInTime { get; set; }

    /// <summary>
    /// Gets the collection of all library memberships of the user.
    /// </summary>
    /// <remarks>
    /// This collection includes private libraries that are not shared.
    /// </remarks>
    public ICollection<Membership> Memberships { get; } = null!;
}
