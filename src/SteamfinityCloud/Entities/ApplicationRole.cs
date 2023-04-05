using Microsoft.AspNetCore.Identity;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a user role in the Steamfinity Cloud authorization system.
/// </summary>
/// <remarks>
/// This is an internal database entity and should not be exposed to the client.
/// </remarks>
public sealed class ApplicationRole : IdentityRole<Guid> { }
