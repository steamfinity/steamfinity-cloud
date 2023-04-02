namespace Steamfinity.Cloud.Services;

/// <summary>
/// Initializes the roles in the Steamfinity Cloud authorization system.
/// </summary>
public interface IRoleInitializer
{
    /// <summary>
    /// Initializes the roles in the Steamfinity Cloud authorization system. Roles that already exist will be skipped.
    /// Make sure to call this method on application startup.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task InitializeRolesAsync();
}
