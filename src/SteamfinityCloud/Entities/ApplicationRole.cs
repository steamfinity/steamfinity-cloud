using Microsoft.AspNetCore.Identity;

namespace Steamfinity.Cloud.Entities;

/// <summary>
/// Represents a user role in the Steamfinity Cloud authorization system.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid> { }
