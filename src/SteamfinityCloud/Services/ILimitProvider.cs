namespace Steamfinity.Cloud.Services;

/// <summary>
/// Represents a service that provides the limit values for various user actions across the app.
/// </summary>
public interface ILimitProvider
{
    /// <summary>
    /// Gets the maximum number of libraries a single user can be a member of.
    /// </summary>
    /// <remarks>
    /// This limit applies to both owned and shared libraries.
    /// </remarks>
    int MaxLibrariesPerUser { get; }

    /// <summary>
    /// Gets the maximum number of members a single library can have, including its owner.
    /// </summary>
    int MaxMembersPerLibrary { get; }

    /// <summary>
    /// Gets the maximum number of accounts that can be added to a single library.
    /// </summary>
    int MaxAccountsPerLibrary { get; }
}
