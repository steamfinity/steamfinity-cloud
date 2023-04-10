using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Models;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(PolicyNames.Users)]
public sealed class UsersController : SteamfinityController
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    [HttpGet("search/{userName}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSearchResult>> FindUserByNameAsync([Required] string userName)
    {
        ArgumentNullException.ThrowIfNull(userName, nameof(userName));

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "There is no user with this username.");
        }

        var result = new UserSearchResult
        {
            UserId = user.Id
        };

        return Ok(result);
    }

    [HttpPatch("{userId}/username")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserNameAsync(Guid userId, UserNameChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "There is no user with this identifier.");
        }

        if (userId != UserId && !IsAdministrator)
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to change other user's name.");
        }

        var result = await _userManager.SetUserNameAsync(user, request.NewUserName);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "DuplicateUserName")
            {
                return ApiError(StatusCodes.Status409Conflict, "DUPLICATE_USERNAME", "There is already a user with this username.");
            }

            if (errorCode == "InvalidUserName")
            {
                return ApiError(StatusCodes.Status400BadRequest, "INVALID_USERNAME", "The username is too short, too long, or contains illegal characters.");
            }

            throw new IdentityException(errorCode);
        }

        return NoContent();
    }

    [HttpPatch("{userId}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePasswordAsync(Guid userId, PasswordChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "There is no user with this identifier.");
        }

        if (userId != UserId && !IsAdministrator)
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to change other users' passwords.");
        }

        IdentityResult result;
        if (IsAdministrator)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
        }
        else
        {
            if (request.CurrentPassword == null)
            {
                return ApiError(StatusCodes.Status401Unauthorized, "INCORRECT_PASSWORD", "The current password must be provided.");
            }

            result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        }

        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "PasswordMismatch")
            {
                return ApiError(StatusCodes.Status401Unauthorized, "INCORRECT_PASSWORD", "The current password is incorrect.");
            }

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
               "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                return ApiError(StatusCodes.Status400BadRequest, "PASSWORD_TOO_WEAK", "The new password is too weak.");
            }

            throw new IdentityException(errorCode);
        }

        return NoContent();
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserAsync(Guid userId, UserDeletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "There is no user with this identifier.");
        }

        if (userId != UserId && !IsAdministrator)
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to delete other users.");
        }

        if (!IsAdministrator && !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return ApiError(StatusCodes.Status401Unauthorized, "INCORRECT_PASSWORD", "The provided password is incorrect.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;
            throw new IdentityException(errorCode);
        }

        return NoContent();
    }
}
