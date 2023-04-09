using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record UserNameChangeRequest
{
    [Required]
    [MaxLength(OtherConstants.DefaultMaximumLenght)]
    public required string NewUserName { get; init; }
}
