using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents a user sign-in request.
/// </summary>
public sealed record SignInRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string UserName { get; init; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string Password { get; init; }
}
