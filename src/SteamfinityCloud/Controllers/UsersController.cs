using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Extensions;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;
using Steamfinity.Cloud.Utilities;
using System.Data;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(PolicyNames.Users)]
public sealed class UsersController : SteamfinityController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IMembershipManager _membershipManager;
    private readonly IAccountManager _accountManager;
    private readonly IAccountInteractionManager _accountInteractionManager;
    private readonly IAuditLog _auditLog;
    private readonly ISteamApi _steamApi;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IMembershipManager membershipManager,
        IAccountManager accountManager,
        IAccountInteractionManager accountInteractionManager,
        IAuditLog auditLog,
        ISteamApi steamApi)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _accountInteractionManager = accountInteractionManager ?? throw new ArgumentNullException(nameof(accountInteractionManager));
        _auditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
        _steamApi = steamApi ?? throw new ArgumentNullException(nameof(steamApi));
    }

    [HttpGet]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IAsyncEnumerable<UserOverview>> GetAllUserOverviewsAsync([FromQuery] PageOptions pageOptions)
    {
        ArgumentNullException.ThrowIfNull(pageOptions, nameof(pageOptions));

        var overviews = _userManager.Users
                        .AsNoTracking()
                        .ApplyPageOptions(pageOptions)
                        .Select(u => new UserOverview
                        {
                            Id = u.Id,
                            UserName = u.UserName
                        })
                        .AsAsyncEnumerable();

        return Ok(overviews);
    }

    [HttpGet("search/{userName}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserSearchResult>> FindUserByNameAsync(string userName)
    {
        ArgumentNullException.ThrowIfNull(userName, nameof(userName));

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundByName;
        }

        var result = new UserSearchResult
        {
            UserId = user.Id
        };

        return Ok(result);
    }

    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetails>> GetCurrentUserOverview()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return CommonApiErrors.CurrentUserNotFound;
        }

        var overview = CreateUserOverview(user);
        return Ok(overview);
    }

    [HttpGet("current/details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetails>> GetCurrentUserDetailsAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return CommonApiErrors.CurrentUserNotFound;
        }

        var details = await CreateUserDetailsAsync(user);
        return Ok(details);
    }

    [HttpGet("{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserOverview>> GetUserOverviewAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        var overview = CreateUserOverview(user);
        return Ok(overview);
    }

    [HttpGet("{userId}/details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDetails>> GetUserDetailsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        if (!IsAdministrator && userId != UserId)
        {
            return CommonApiErrors.AccessDenied;
        }

        var userDetails = await CreateUserDetailsAsync(user);
        return Ok(userDetails);
    }

    [HttpGet("{userId}/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<AccountOverview>>> GetAccountsAsync(Guid userId, [FromQuery] AccountQueryOptions queryOptions, [FromQuery] PageOptions pageOptions, bool refreshAccounts = false)
    {
        ArgumentNullException.ThrowIfNull(queryOptions, nameof(queryOptions));
        ArgumentNullException.ThrowIfNull(pageOptions, nameof(pageOptions));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        if (!IsAdministrator && userId != UserId)
        {
            return CommonApiErrors.AccessDenied;
        }

        var authorizedLibraries = _membershipManager.Memberships
                                  .AsNoTracking()
                                  .Where(m => m.UserId == userId)
                                  .Select(m => m.LibraryId);

        var accounts = _accountManager.Accounts
                       .AsNoTracking()
                       .Where(a => authorizedLibraries.Contains(a.LibraryId))
                       .ApplyQueryOptions(queryOptions)
                       .ApplyPageOptions(pageOptions);

        if (refreshAccounts)
        {
            await _steamApi.RefreshAccountsAsync(accounts);
        }

        var accountOverviews = accounts
                               .Select(a => new AccountOverview
                               {
                                   Id = a.Id,
                                   ProfileName = a.ProfileName,
                                   AvatarUrl = a.AvatarUrl
                               })
                               .AsAsyncEnumerable();

        return Ok(accountOverviews);
    }

    [HttpGet("{userId}/favorites")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<AccountOverview>>> GetFavoriteAccountsAsync(Guid userId, [FromQuery] AccountQueryOptions queryOptions, [FromQuery] PageOptions pageOptions, bool refreshAccounts = false)
    {
        ArgumentNullException.ThrowIfNull(queryOptions, nameof(queryOptions));
        ArgumentNullException.ThrowIfNull(pageOptions, nameof(pageOptions));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        if (!IsAdministrator && userId != UserId)
        {
            return CommonApiErrors.AccessDenied;
        }

        var authorizedLibraries = _membershipManager.Memberships
                                  .AsNoTracking()
                                  .Where(m => m.UserId == userId)
                                  .Select(m => m.LibraryId);

        var accounts = _accountInteractionManager.Interactions
                       .AsNoTracking()
                       .Where(i => i.UserId == userId && i.IsFavorite)
                       .Include(i => i.Account)
                       .Select(i => i.Account)
                       .Where(a => authorizedLibraries.Contains(a.LibraryId))
                       .ApplyQueryOptions(queryOptions)
                       .ApplyPageOptions(pageOptions);

        if (refreshAccounts)
        {
            await _steamApi.RefreshAccountsAsync(accounts);
        }

        var accountOverviews = accounts
                               .Select(a => new AccountOverview
                               {
                                   Id = a.Id,
                                   ProfileName = a.ProfileName,
                                   AvatarUrl = a.AvatarUrl
                               })
                               .AsAsyncEnumerable();

        return Ok(accountOverviews);
    }

    [HttpGet("{userId}/hashtags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<string>>> GetHashtagsAsync(Guid userId, [FromQuery] PageOptions pageOptions)
    {
        ArgumentNullException.ThrowIfNull(pageOptions, nameof(pageOptions));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        if (!IsAdministrator && userId != UserId)
        {
            return CommonApiErrors.AccessDenied;
        }

        var hashtags = _membershipManager.Memberships
                       .AsNoTracking()
                       .Where(m => m.UserId == userId)
                       .Include(m => m.Library)
                       .ThenInclude(l => l.Accounts)
                       .ThenInclude(a => a.Hashtags)
                       .SelectMany(m => m.Library.Accounts.SelectMany(a => a.Hashtags))
                       .Select(h => h.Name)
                       .Distinct()
                       .ApplyPageOptions(pageOptions)
                       .AsAsyncEnumerable();

        return Ok(hashtags);
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
            return CommonApiErrors.UserNotFoundById;
        }

        if (!IsAdministrator && userId != UserId)
        {
            return CommonApiErrors.AccessDenied;
        }

        var previousUserName = user.UserName;
        if (request.NewUserName == previousUserName)
        {
            return NoContent();
        }

        var result = await _userManager.SetUserNameAsync(user, request.NewUserName);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "DuplicateUserName")
            {
                return CommonApiErrors.DuplicateUserName;
            }

            if (errorCode == "InvalidUserName")
            {
                return CommonApiErrors.InvalidUserName;
            }

            throw new IdentityException(errorCode);
        }

        await _auditLog.LogUserNameChangeAsync(UserId, user.Id, previousUserName!, user.UserName!);
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
            return CommonApiErrors.UserNotFoundById;
        }

        if (!IsAdministrator && userId != UserId)
        {
            return CommonApiErrors.AccessDenied;
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
                return CommonApiErrors.IncorrectPassword;
            }

            result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        }

        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;

            if (errorCode == "PasswordMismatch")
            {
                return CommonApiErrors.IncorrectPassword;
            }

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
               "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                return CommonApiErrors.PasswordTooWeak;
            }

            throw new IdentityException(errorCode);
        }

        await _auditLog.LogUserPasswordChangeAsync(UserId, user.Id);
        return NoContent();
    }

    [HttpPost("{userId}/roles")]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToRoleAsync(Guid userId, UserRoleAdditionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        var role = await _roleManager.FindByNameAsync(request.RoleName);
        if (role == null)
        {
            return CommonApiErrors.RoleNotFound;
        }

        if (await _userManager.IsInRoleAsync(user, request.RoleName))
        {
            return CommonApiErrors.UserAlreadyInRole;
        }

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;
            throw new IdentityException(errorCode);
        }

        await _auditLog.LogUserRoleAddition(UserId, user.Id, role.Name ?? role.Id.ToString());
        return NoContent();
    }

    [HttpDelete("{userId}/roles/{roleName}")]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFromRoleAsync(Guid userId, string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName, nameof(roleName));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return CommonApiErrors.RoleNotFound;
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            return CommonApiErrors.UserNotInRole;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;
            throw new IdentityException(errorCode);
        }

        await _auditLog.LogUserRoleRemoval(UserId, user.Id, role.Name ?? role.Id.ToString());
        return NoContent();
    }

    [HttpPatch("{userId}/is-suspended")]
    [Authorize(PolicyNames.Administrators)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserSuspensionAsync(Guid userId, UserSuspensionChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        var previousIsSuspended = user.IsSuspended;
        if (request.NewIsSuspended == previousIsSuspended)
        {
            return NoContent();
        }

        user.IsSuspended = request.NewIsSuspended;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;
            throw new IdentityException(errorCode);
        }

        await _auditLog.LogUserSuspensionChangeAsync(UserId, user.Id, previousIsSuspended, user.IsSuspended);
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
            return CommonApiErrors.UserNotFoundById;
        }

        if (!IsAdministrator && userId != UserId)
        {
            return CommonApiErrors.AccessDenied;
        }

        if (!IsAdministrator && !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return CommonApiErrors.IncorrectPassword;
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorCode = result.Errors.First().Code;
            throw new IdentityException(errorCode);
        }

        await _auditLog.LogUserDeletionAsync(UserId, user.Id);
        return NoContent();
    }

    private static UserOverview CreateUserOverview(ApplicationUser user)
    {
        return new UserOverview
        {
            Id = user.Id,
            UserName = user.UserName
        };
    }

    private async Task<UserDetails> CreateUserDetailsAsync(ApplicationUser user)
    {
        return new UserDetails
        {
            Id = user.Id,
            UserName = user.UserName,
            SignUpTime = user.SignUpTime,
            Roles = await _userManager.GetRolesAsync(user)
        };
    }
}
