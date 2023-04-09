using Steamfinity.Cloud.Constants;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record UserNameChangeRequest
{
    [Required]
    [MinLength(PropertyLengthConstraints.MinUserNameLength)]
    [MaxLength(PropertyLengthConstraints.MaxUserNameLength)]
    public required string NewUserName { get; init; }
}
