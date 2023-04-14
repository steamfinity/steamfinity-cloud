using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Extensions;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;
using System.Data;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/libraries")]
[Authorize(PolicyNames.Users)]
public sealed class LibrariesController : SteamfinityController
{
    private readonly ILibraryManager _libraryManager;
    private readonly IMembershipManager _membershipManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IAccountManager _accountManager;
    private readonly ISteamApi _steamApi;

    public LibrariesController(
        ILibraryManager libraryManager,
        IMembershipManager membershipManager,
        IPermissionManager permissionManager,
        IAccountManager accountManager,
        ISteamApi steamApi)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _steamApi = steamApi ?? throw new ArgumentNullException(nameof(steamApi));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IAsyncEnumerable<LibraryOverview>> GetLibrariesAsync()
    {
        var libraries = _membershipManager.Memberships
                        .AsNoTracking()
                        .Where(m => m.UserId == UserId)
                        .Include(m => m.Library)
                        .Select(m => new LibraryOverview
                        {
                            Id = m.LibraryId,
                            Name = m.Library.Name
                        })
                        .AsAsyncEnumerable();

        return Ok(libraries);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateLibraryAsync(LibraryCreationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var library = new Library
        {
            Name = request.Name,
            Description = request.Description
        };

        var result = await _libraryManager.CreateAsync(library, UserId);

        if (result == LibraryCreationResult.LibraryLimitExceeded)
        {
            return ApiError(StatusCodes.Status403Forbidden, "LIBRARY_LIMIT_EXCEEDED", "You are already a member of the maximum number of libraries.");
        }

        return NoContent();
    }

    [HttpGet("{libraryId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LibraryOverview>> GetLibraryAsync(Guid libraryId)
    {
        var library = await _libraryManager.FindByIdAsync(libraryId);
        if (library == null)
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not a member of this library.");
        }

        var details = new LibraryDetails
        {
            Id = library.Id,
            Name = library.Name,
            Description = library.Description,
            Role = (await _membershipManager.FindByIdAsync(libraryId, UserId))!.Role,
            CreationTime = library.CreationTime
        };

        return Ok(details);
    }

    [HttpPatch("{libraryId}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeLibraryNameAsync(Guid libraryId, LibraryNameChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var library = await _libraryManager.FindByIdAsync(libraryId);
        if (library == null)
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage this library.");
        }

        library.Name = request.NewName;
        await _libraryManager.UpdateAsync(library);

        return NoContent();
    }

    [HttpPatch("{libraryId}/description")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ChangeLibraryDescriptionAsync(Guid libraryId, LibraryDescriptionChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var library = await _libraryManager.FindByIdAsync(libraryId);
        if (library == null)
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage this library.");
        }

        library.Description = request.NewDescription;
        await _libraryManager.UpdateAsync(library);

        return NoContent();
    }

    [HttpGet("{libraryId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<MemberOverview>>> GetMembersAsync(Guid libraryId)
    {
        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not a member of this library.");
        }

        var overviews = _membershipManager.Memberships
                        .AsNoTracking()
                        .Where(m => m.LibraryId == libraryId)
                        .Include(m => m.User)
                        .Select(m => new MemberOverview
                        {
                            Id = m.UserId,
                            UserName = m.User.UserName,
                            Role = m.Role,
                        })
                        .AsAsyncEnumerable();

        return Ok(overviews);
    }

    [HttpPost("{libraryId}/members")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddMemberAsync(Guid libraryId, MemberAdditionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage users in this library.");
        }

        var result = await _membershipManager.AddMemberAsync(libraryId, request.UserId, request.Role);

        if (result == MemberAdditionResult.LibraryNotFound)
        {
            return LibraryNotFoundError();
        }

        if (result == MemberAdditionResult.UserNotFound)
        {
            return ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "There is no user with this identifier.");
        }

        if (result == MemberAdditionResult.MemberAlreadyAdded)
        {
            return ApiError(StatusCodes.Status400BadRequest, "USER_ALREADY_ADDED", "This user is already a member of the library.");
        }

        if (result == MemberAdditionResult.MemberLimitExceeded)
        {
            return ApiError(StatusCodes.Status403Forbidden, "MEMBER_LIMIT_EXCEEDED", "This library already has the maximum number of members.");
        }

        return NoContent();
    }

    [HttpPatch("{libraryId}/members/{userId}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeMemberRoleAsync(Guid libraryId, Guid userId, MemberRoleChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage users in this library.");
        }

        var result = await _membershipManager.ChangeMemberRoleAsync(libraryId, userId, request.NewRole);

        if (result == MemberRoleChangeResult.LibraryNotFound)
        {
            return LibraryNotFoundError();
        }

        if (result == MemberRoleChangeResult.UserNotFound)
        {
            return ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "There is no user with the this identifier.");
        }

        if (result == MemberRoleChangeResult.UserNotMember)
        {
            return ApiError(StatusCodes.Status400BadRequest, "USER_NOT_MEMBER", "This user is not a member of the library.");
        }

        return NoContent();
    }

    [HttpDelete("{libraryId}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveMemberAsync(Guid libraryId, Guid userId)
    {
        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status404NotFound, "ACCESS_DENIED", "You are not allowed to manage users in this library.");
        }

        var result = await _membershipManager.RemoveMemberAsync(libraryId, userId);

        if (result == MemberRemovalResult.LibraryNotFound)
        {
            return ApiError(StatusCodes.Status404NotFound, "LIBRARY_NOT_FOUND", "There is no library with this identifier.");
        }

        if (result == MemberRemovalResult.UserNotFound)
        {
            return LibraryNotFoundError();
        }

        if (result == MemberRemovalResult.UserNotMember)
        {
            return ApiError(StatusCodes.Status400BadRequest, "USER_NOT_MEMBER", "This user is not a member of the library.");
        }

        return NoContent();
    }

    [HttpGet("{libraryId}/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<AccountOverview>>> GetAccountsAsync(Guid libraryId, [FromQuery] AccountQueryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not a member of this library.");
        }

        var accountOverviews = _accountManager.Accounts
                               .AsNoTracking()
                               .Where(a => a.LibraryId == libraryId)
                               .ApplyQueryOptions(options)
                               .Select(a => new AccountOverview
                               {
                                   Id = a.Id,
                                   ProfileName = a.ProfileName,
                                   AvatarUrl = a.AvatarUrl
                               })
                                .AsAsyncEnumerable();

        return Ok(accountOverviews);
    }

    [HttpPost("{libraryId}/accounts")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddAccountAsync(Guid libraryId, AccountAdditionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return ApiError(StatusCodes.Status404NotFound, "LIBRARY_NOT_FOUND", "There is no library with this identifier.");
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage accounts in this library.");
        }

        var steamId = await _steamApi.TryResolveSteamIdAsync(request.SteamId);
        if (steamId == null)
        {
            return ApiError(StatusCodes.Status400BadRequest, "INVALID_STEAMID", "The provided STEAM ID is invalid.");
        }

        var account = new Account
        {
            LibraryId = libraryId,
            SteamId = steamId.Value
        };

        var result = await _accountManager.AddAsync(account);

        if (result == AccountAdditionResult.LibraryNotFound)
        {
            return ApiError(StatusCodes.Status404NotFound, "LIBRARY_NOT_FOUND", "There is no library with this identifier.");
        }

        if (result == AccountAdditionResult.LibrarySizeExceeded)
        {
            return ApiError(StatusCodes.Status403Forbidden, "LIBRARY_SIZE_EXCEEDED", "This library already has the maximum number of accounts.");
        }

        if (result == AccountAdditionResult.InvalidSteamId)
        {
            return ApiError(StatusCodes.Status400BadRequest, "INVALID_STEAMID", "The provided STEAM ID is invalid.");
        }

        return NoContent();
    }

    [HttpDelete("{libraryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteLibraryAsync(Guid libraryId)
    {
        var library = await _libraryManager.FindByIdAsync(libraryId);
        if (library == null)
        {
            return LibraryNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage this library.");
        }

        await _libraryManager.DeleteAsync(library);
        return NoContent();
    }

    private static ObjectResult LibraryNotFoundError()
    {
        return ApiError(StatusCodes.Status404NotFound, "LIBRARY_NOT_FOUND", "There is no library with this identifier.");
    }
}
