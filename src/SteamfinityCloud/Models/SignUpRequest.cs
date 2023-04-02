using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

/// <summary>
/// Represents a user sign-up request.
/// </summary>
public sealed record SignUpRequest
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

    /// <summary>
    /// Gets or sets the administrator sign-up key.<br/>
    /// When the correct administrator sign-up key is provided, the user is automatically added<br/>
    /// to the administrator role.
    /// </summary>
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string AdministratorSignUpKey { get; init; }
}
