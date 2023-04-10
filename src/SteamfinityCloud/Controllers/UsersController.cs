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

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    private Guid CurrentUserId
    {
        get
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The user authentication token is missing the NameIdentifier claim.");

            if (!Guid.TryParse(nameIdentifier, out var userId))
            {
                throw new InvalidOperationException("The user authentication token NameIdentifier claim is not a valid GUID.");
            }

            return userId;
        }
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserNameAsync(Guid userId, UserNameChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-not-found");
        }

        if (userId != CurrentUserId && !IsCurrentUserAdministrator())
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var result = await _userManager.SetUserNameAsync(user, request.NewUserName);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "DuplicateUserName")
            {
                return Problem(statusCode: StatusCodes.Status409Conflict, detail: "duplicate-user-name");
            }

            if (errorCode == "InvalidUserName")
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-user-name");
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
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePasswordAsync(Guid userId, PasswordChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-not-found");
        }

        if (userId != CurrentUserId && !IsCurrentUserAdministrator())
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "PasswordMismatch")
            {
                return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-password");
            }

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
               "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "password-too-weak");
            }

            throw new IdentityException(errorCode);
        }

        return NoContent();
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserAsync(Guid userId, UserDeletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-not-found");
        }

        if (userId != CurrentUserId && !IsCurrentUserAdministrator())
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-password");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;
            throw new IdentityException(errorCode);
        }

        return NoContent();
    }

    private bool IsCurrentUserAdministrator()
    {
        return User.HasClaim(ClaimTypes.Role, RoleNames.Administrator);
    }
}
