using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents an administrator's request to reset a user's password.
/// </summary>
public sealed record PasswordResetRequest
{
    /// <summary>
    /// Gets or sets the new password that the user will have when the request is accepted.
    /// </summary>
    /// <value>The new user's password.</value>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewPassword { get; init; }
}
