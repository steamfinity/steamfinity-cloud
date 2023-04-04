using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents an administrator's request to change a user's email address.
/// </summary>
public sealed record ForceEmailChangeRequest
{
    /// <summary>
    /// Gets or sets the new email address that the user will have when the request is accepted.
    /// </summary>
    /// <value>The new user's email.</value>
    [Required]
    [EmailAddress]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewEmail { get; init; }
}
