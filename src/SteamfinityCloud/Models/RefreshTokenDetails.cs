namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents the details of a refresh token obtained as a result of successful user sign-in.<br/>
/// The refresh token is valid until the user changes their password and can be used to generate<br/>
/// temporary JWT bearer authentication tokens.
/// </summary>
public sealed record RefreshTokenDetails
{
    /// <summary>
    /// Gets or sets the user ID associated with the refresh token.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    public required string RefreshToken { get; init; }
}
