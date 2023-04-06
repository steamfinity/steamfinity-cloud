using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Models;
using System.Security.Claims;

namespace Steamfinity.Cloud.Controllers;

/// <summary>
/// The controller responsible for user account management.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(PolicyNames.Users)]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userManager">The user manager used to manage users.</param>
    /// <param name="logger">The logger used to log controller actions.</param>
    public UsersController(UserManager<ApplicationUser> userManager, ILogger<UsersController> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Changes the username of the currently authenticated user.
    /// </summary>
    /// <param name="request">The user name change request model, containing the new username.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException">Throw when an unhandled identity error occurs.</exception>
    [HttpPatch("current/user-name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserNameAsync(UserNameChangeRequest request)
    {
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

    /// <summary>
    /// Changes the username of the specified user.
    /// </summary>
    /// <remarks>Calling this method requires the user to have the administrator role.</remarks>
    /// <param name="userId">The identifier of the user whose name is being changed.</param>
    /// <param name="request">The user name change request model, containing the new username.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException">Thrown when an unhandled identity error occurs.</exception>
    [HttpPatch("{userId}/user-name")]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserNameAsync(Guid userId, UserNameChangeRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var administratorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The administrator '{administratorId}' has attempted to change the username of the non-existent user '{userId}' to '{newUserName}'.", administratorId, userId, request.NewUserName);
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        var result = await _userManager.SetUserNameAsync(user, request.NewUserName);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "DuplicateUserName")
            {
                _logger.LogInformation("The administrator '{administratorId}' has attempted to change the username of the user '{userId}' to '{newUserName}', but this username is already taken.", administratorId, user.Id, request.NewUserName);
                return Problem(statusCode: StatusCodes.Status409Conflict, detail: "duplicate-user-name");
            }

            if (errorCode == "InvalidUserName")
            {
                _logger.LogInformation("The administrator '{administratorId}' has attempted to change the username of the user '{userId}' to '{newUserName}', but this username is invalid.", administratorId, user.Id, request.NewUserName);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-user-name");
            }

            _logger.LogError("An identity error has occurred while attempting to change the username of the user '{userId}' to '{newUserName}': '{errorCode}'.", user.Id, request.NewUserName, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The administrator '{administratorId}' has successfully changed the username of the user '{userId}' to '{newUserName}'.", administratorId, user.Id, request.NewUserName);
        return NoContent();
    }

    /// <summary>
    /// Changes the email address of the currently authenticated user.
    /// </summary>
    /// <param name="request">The email address change request model, containing the new email address.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException">Thrown when an unhandled identity error occurs.</exception>
    [HttpPatch("current/email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeEmailAsync(EmailChangeRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The deleted user '{userId}' has attempted to change their email to '{newEmail}'.", userId, request.NewEmail);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-not-found");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogInformation("The user '{userId}' has attempted to change their email to '{newEmail} but provided an incorrect password.", user.Id, request.NewEmail);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-password");
        }

        var result = await _userManager.SetEmailAsync(user, request.NewEmail);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "DuplicateEmail")
            {
                _logger.LogInformation("The user '{userId}' has attempted to change their email to '{newEmail}', but this email is already associated with another user.", user.Id, request.NewEmail);
                return Problem(statusCode: StatusCodes.Status409Conflict, detail: "duplicate-email");
            }

            if (errorCode == "InvalidEmail")
            {
                _logger.LogInformation("The user '{userId}' has attempted to change their email to '{newEmail}', but this email is invalid.", user.Id, request.NewEmail);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-email");
            }

            _logger.LogError("An identity error has occurred while attempting to change the email of the user '{userId}' to '{newEmail}': '{errorCode}'.", user.Id, request.NewEmail, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The user '{userId}' has successfully changed their email to '{newEmail}'.", user.Id, request.NewEmail);
        return NoContent();
    }

    /// <summary>
    /// Changes the email address of the specified user.
    /// </summary>
    /// <remarks>Calling this method requires the user to have the administrator role.</remarks>
    /// <param name="userId">The identifier of the user whose email address is being changed.</param>
    /// <param name="request">The email address change request model, containing the new email address.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException"></exception>
    [HttpPatch("{userId}/email")]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeEmailAsync(Guid userId, ForceEmailChangeRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var administratorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The administrator '{administratorId}' has attempted to change the email of the non-existent user '{userId}' to '{newEmail}'.", administratorId, userId, request.NewEmail);
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        var result = await _userManager.SetEmailAsync(user, request.NewEmail);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "DuplicateEmail")
            {
                _logger.LogInformation("The administrator '{administratorId}' has attempted to change the email of the user '{userId}' to '{newEmail}', but this email is already associated with another user.", administratorId, user.Id, request.NewEmail);
                return Problem(statusCode: StatusCodes.Status409Conflict, detail: "duplicate-email");
            }

            if (errorCode == "InvalidEmail")
            {
                _logger.LogInformation("The administrator '{administratorId}' has attempted to change the email of the user '{userId}' to '{newEmail}', but this email is invalid.", administratorId, user.Id, request.NewEmail);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-email");
            }

            _logger.LogError("An identity error has occurred while attempting to change the email of the user '{userId}' to '{newEmail}': '{errorCode}'.", user.Id, request.NewEmail, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The administrator '{administratorId}' has successfully changed the email of the user '{userId}' to '{newEmail}'.", administratorId, user.Id, request.NewEmail);
        return NoContent();
    }

    /// <summary>
    /// Changes the password of the currently authenticated users.
    /// </summary>
    /// <param name="request">The password change request model, containing both current password and the new password.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException">Thrown when an unhandled identity error occurs.</exception>
    [HttpPatch("current/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePasswordAsync(PasswordChangeRequest request)
    {
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

    /// <summary>
    /// Resets the password of the specified user.
    /// </summary>
    /// <remarks>Calling this method requires the user to have the administrator role.</remarks>
    /// <param name="userId">The identifier of the user whose password is being reset.</param>
    /// <param name="request">The password reset request model, containing the new password.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException">Thrown when an unhandled identity error occurs.</exception>
    [HttpPatch("{userId}/password")]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPasswordAsync(Guid userId, PasswordResetRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var administratorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The administrator '{administratorId}' has attempted to reset the password of the non-existent user '{userId}'.", administratorId, userId);
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
               "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                _logger.LogInformation("The administrator '{administrator}' has attempted to reset the password of the user '{userId}', but the provided new password is too weak.", administratorId, user.Id);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "password-too-weak");
            }

            _logger.LogError("An identity error has occurred while attempting to reset the password of user '{userId}': '{errorCode}'.", user.Id, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The administrator '{administratorId}' has successfully reset the password of the user '{userId}'.", administratorId, user.Id);
        return NoContent();
    }

    /// <summary>
    /// Permanently deletes the account of the currently authenticated user.
    /// </summary>
    /// <param name="request">The user deletion request model containing the user's password.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException">Thrown when an unhandled identity error occurs.</exception>
    [HttpDelete("current")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserAsync(UserDeletionRequest request)
    {
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

    /// <summary>
    /// Permanently deletes the account of the specified user.
    /// </summary>
    /// <remarks>Calling this method requires the user to have the administrator role.</remarks>
    /// <param name="userId">The identifier of the user whose account is being deleted.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, containing the action result.</returns>
    /// <exception cref="IdentityException">Thrown when an unhandled identity error occurs.</exception>
    [HttpDelete("{userId}")]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var administratorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user == null)
        {
            _logger.LogInformation("The administrator '{administratorId}' has attempted to delete the non-existent user '{userId}'.", administratorId, userId);
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            _logger.LogError("An identity error has occurred while attempting to delete the user '{userId}': '{errorCode}'.", user.Id, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The administrator '{administratorId}' has successfully deleted the user '{userId}'.", administratorId, user.Id);
        return NoContent();
    }
}
