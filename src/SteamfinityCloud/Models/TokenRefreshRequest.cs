using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents a user's request to refresh their temporary JWT bearer authentication token.
/// </summary>

public sealed record TokenRefreshRequest
{
    /// <summary>
    /// Gets or sets the user ID associated with the refresh token.
    /// </summary>
    [Required]
    public required Guid UserId { get; init; }

    /// <summary>
    /// Gets or sets the refresh token used to refresh the authentication token.
    /// </summary>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string RefreshToken { get; init; }
}
