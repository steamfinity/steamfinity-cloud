using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(PolicyNames.Users)]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger _logger;

    public UsersController(UserManager<ApplicationUser> userManager, ILogger<UsersController> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("find-by-user-name")]
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        var result = new UserSearchResult
        {
            UserId = user.Id
        };

        return Ok(result);
    }

    [HttpPatch("current/user-name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserNameAsync(UserNameChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.GetUserAsync(User);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The deleted user '{userId}' has attempted to change their username to '{newUserName}'.", userId, request.NewUserName);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-not-found");
        }

        var result = await _userManager.SetUserNameAsync(user, request.NewUserName);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "DuplicateUserName")
            {
                _logger.LogInformation("The user '{userId}' has attempted to change their username to '{newUserName}', but this username is already taken.", user.Id, request.NewUserName);
                return Problem(statusCode: StatusCodes.Status409Conflict, detail: "duplicate-user-name");
            }

            if (errorCode == "InvalidUserName")
            {
                _logger.LogInformation("The user '{userId}' has attempted to change their username to '{newUserName}', but this username is invalid.", user.Id, request.NewUserName);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-user-name");
            }

            _logger.LogError("An identity error has occurred while attempting to change the username of the user '{userId}' to '{newUserName}': '{errorCode}'.", user.Id, request.NewUserName, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The user '{userId}' has successfully changed their username to '{newUserName}'.", user.Id, request.NewUserName);
        return NoContent();
    }

    [HttpPatch("current/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePasswordAsync(PasswordChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.GetUserAsync(User);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The deleted user '{userId}' has attempted to change their password.", userId);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-not-found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "PasswordMismatch")
            {
                _logger.LogInformation("The user '{userId}' has attempted to change their password, but the provided current password is incorrect.", user.Id);
                return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-password");
            }

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
               "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                _logger.LogInformation("The user '{userId}' has attempted to change their password, but the provided new password is too weak.", user.Id);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "password-too-weak");
            }

            _logger.LogError("An identity error has occurred while attempting to change the password of the user '{userId}': '{errorCode}'.", user.Id, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The user '{userId}' has successfully changed their password.", user.Id);
        return NoContent();
    }

    [HttpDelete("current")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserAsync(UserDeletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.GetUserAsync(User);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The deleted user '{userId}' has attempted to delete their account again.", userId);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-not-found");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogInformation("The user '{userId}' has attempted to delete their account but provided an incorrect password.", user.Id);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-password");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            _logger.LogError("An identity error has occurred while attempting to delete the user '{userId}': '{errorCode}'.", user.Id, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The user '{userId}' has been successfully deleted.", user.Id);
        return NoContent();
    }
}
