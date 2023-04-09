using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record SignInRequest
{
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string UserName { get; init; }

    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string Password { get; init; }
}
