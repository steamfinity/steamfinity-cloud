using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents a user's request to delete their account.
/// </summary>
public sealed record UserDeletionRequest
{
    /// <summary>
    /// Gets or sets the user's password. The correct password must be provided. Otherwise, the request won't be accepted.
    /// </summary>
    /// <value> The user's password.</value>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string Password { get; init; }
}
