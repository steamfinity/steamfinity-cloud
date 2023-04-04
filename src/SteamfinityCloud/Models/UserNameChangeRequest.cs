using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents a user's request to change their username.
/// </summary>
public sealed record UserNameChangeRequest
{
    /// <summary>
    /// Gets or sets the new username that the user will have when the request is accepted.
    /// </summary>
    /// <value>The new username.</value>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewUserName { get; init; }
}
