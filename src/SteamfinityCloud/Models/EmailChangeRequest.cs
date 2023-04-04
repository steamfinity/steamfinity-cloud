using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents a user's request to change their email address.
/// </summary>
public sealed record EmailChangeRequest
{
    /// <summary>
    /// Gets or sets the new email address that the user will have when the request is accepted.
    /// </summary>
    /// <value>The new user's email.</value>
    [Required]
    [EmailAddress]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewEmail { get; init; }

    /// <summary>
    /// Gets or sets the user's password. The correct password must be provided. Otherwise, the request won't be accepted.
    /// </summary>
    /// <value>The user's password.</value>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string Password { get; init; }
}
