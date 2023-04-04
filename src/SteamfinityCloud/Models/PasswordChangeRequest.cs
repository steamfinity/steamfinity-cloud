using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents a user's request to change their password.
/// </summary>
public sealed record PasswordChangeRequest
{
    /// <summary>
    /// Gets or sets the current user's password. The correct password must be provided. Otherwise, the request won't be accepted.
    /// </summary>
    /// <value>The current user's password.</value>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string CurrentPassword { get; init; }

    /// <summary>
    /// Gets or sets the new password that the user will have when the request is accepted.
    /// </summary>
    /// <value>The new user's password.</value>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewPassword { get; init; }
}
