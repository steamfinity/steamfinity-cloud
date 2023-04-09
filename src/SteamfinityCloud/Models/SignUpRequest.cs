using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record SignUpRequest
{
    [Required]
    [MinLength(PropertyLengthConstraints.MinUserNameLength)]
    [MaxLength(PropertyLengthConstraints.MaxUserNameLength)]
    public required string UserName { get; init; }

    [Required]
    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public required string Password { get; init; }

    [MaxLength(PropertyLengthConstraints.MaxOtherLenght)]
    public string? AdministratorSignUpKey { get; init; }
}
