namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents the details of an authentication token.
/// </summary>
public sealed record AuthenticationTokenDetails
{
    /// <summary>
    /// Gets or sets the JWT bearer authentication token.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// Gets or sets the expiration time of the authentication token.
    /// </summary>
    public required DateTimeOffset ExpirationTime { get; init; }

    /// <summary>
    /// Gets or sets the user roles associated with the authentication token.
    /// </summary>
    public required IEnumerable<string> Roles { get; init; }
}
