using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record SignUpRequest
{
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string UserName { get; init; }

    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string Password { get; init; }

    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public string? AdministratorSignUpKey { get; init; }
}
