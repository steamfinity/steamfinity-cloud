using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Extensions;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;
using Steamfinity.Cloud.Utilities;
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
    private readonly IAuditLog _auditLog;
    private readonly ISteamApi _steamApi;

    public LibrariesController(
        ILibraryManager libraryManager,
        IMembershipManager membershipManager,
        IPermissionManager permissionManager,
        IAccountManager accountManager,
        IAuditLog auditLog,
        ISteamApi steamApi)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _auditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
        _steamApi = steamApi ?? throw new ArgumentNullException(nameof(steamApi));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IAsyncEnumerable<LibraryOverview>> GetLibrariesAsync([FromQuery] PageOptions pageOptions)
    {
        ArgumentNullException.ThrowIfNull(pageOptions, nameof(pageOptions));

        var libraryOverviews = _membershipManager.Memberships
                               .AsNoTracking()
                               .Where(m => m.UserId == UserId)
                               .ApplyPageOptions(pageOptions)
                               .Include(m => m.Library)
                               .Select(m => new LibraryOverview
                               {
                                   Id = m.LibraryId,
                                   Name = m.Library.Name
                               })
                               .AsAsyncEnumerable();

        return Ok(libraryOverviews);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
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
            return CommonApiErrors.LibraryLimitExceeded;
        }

        var overview = new LibraryOverview
        {
            Id = library.Id,
            Name = library.Name
        };

        await _auditLog.LogLibraryCreationAsync(UserId, library.Id);
        return CreatedAtAction("GetLibrary", new { LibraryId = library.Id }, overview);
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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var libraryDetails = new LibraryDetails
        {
            Id = library.Id,
            Name = library.Name,
            Description = library.Description,
            Role = (await _membershipManager.FindByIdAsync(libraryId, UserId))?.Role,
            CreationTime = library.CreationTime
        };

        return Ok(libraryDetails);
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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var previosName = library.Name;
        if (request.NewName == previosName)
        {
            return NoContent();
        }

        library.Name = request.NewName;

        await _libraryManager.UpdateAsync(library);
        await _auditLog.LogLibraryNameChangeAsync(UserId, libraryId, previosName, library.Name);

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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        if (request.NewDescription == library.Description)
        {
            return NoContent();
        }

        library.Description = request.NewDescription;

        await _libraryManager.UpdateAsync(library);
        await _auditLog.LogLibraryDescriptionChangeAsync(UserId, libraryId);

        return NoContent();
    }

    [HttpGet("{libraryId}/members")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<MemberOverview>>> GetMembersAsync(Guid libraryId, [FromQuery] PageOptions pageOptions)
    {
        ArgumentNullException.ThrowIfNull(pageOptions, nameof(pageOptions));

        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var overviews = _membershipManager.Memberships
                        .AsNoTracking()
                        .Where(m => m.LibraryId == libraryId)
                        .ApplyPageOptions(pageOptions)
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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var result = await _membershipManager.AddMemberAsync(libraryId, request.UserId, request.Role);

        if (result == MemberAdditionResult.LibraryLimitExceeded)
        {
            return CommonApiErrors.LibraryLimitExceeded;
        }

        if (result == MemberAdditionResult.MemberLimitExceeded)
        {
            return CommonApiErrors.MemberLimitExceeded;
        }

        await _auditLog.LogMemberAdditionAsync(UserId, libraryId, request.UserId);
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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var previousRole = (await _membershipManager.FindByIdAsync(libraryId, userId))?.Role;
        var result = await _membershipManager.ChangeMemberRoleAsync(libraryId, userId, request.NewRole);

        if (result == MemberRoleChangeResult.UserNotMember)
        {
            return CommonApiErrors.UserNotLibraryMember;
        }

        await _auditLog.LogMemberRoleChangeAsync(UserId, libraryId, userId, previousRole!.Value, request.NewRole);
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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        await _membershipManager.RemoveMemberAsync(libraryId, userId);
        await _auditLog.LogMemberRemovalAsync(UserId, libraryId, userId);

        return NoContent();
    }

    [HttpGet("{libraryId}/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<AccountOverview>>> GetAccountsAsync(Guid libraryId, [FromQuery] AccountQueryOptions queryOptions, [FromQuery] PageOptions pageOptions, bool refreshAccounts = false)
    {
        ArgumentNullException.ThrowIfNull(queryOptions, nameof(queryOptions));
        ArgumentNullException.ThrowIfNull(pageOptions, nameof(pageOptions));

        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var accounts = _accountManager.Accounts
                       .AsNoTracking()
                       .Where(a => a.LibraryId == libraryId)
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

    [HttpPost("{libraryId}/accounts")]
    [ProducesResponseType(StatusCodes.Status201Created)]
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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var steamId = await _steamApi.TryResolveSteamIdAsync(request.SteamId);
        if (steamId == null)
        {
            return CommonApiErrors.InvalidSteamId;
        }

        var account = new Account
        {
            LibraryId = libraryId,
            SteamId = steamId.Value
        };

        var result = await _accountManager.AddAsync(account);

        if (result == AccountAdditionResult.AccountLimitExceeded)
        {
            return CommonApiErrors.AccountLimitExceeded;
        }

        if (result == AccountAdditionResult.InvalidSteamId)
        {
            return CommonApiErrors.InvalidSteamId;
        }

        var overview = new AccountOverview
        {
            Id = account.Id,
            ProfileName = account.ProfileName,
            AvatarUrl = account.AvatarUrl
        };

        await _auditLog.LogAccountAdditionAsync(UserId, libraryId, account.Id);
        return CreatedAtAction("GetAccount", "Accounts", new { AccountId = account.Id }, overview);
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
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        await _libraryManager.DeleteAsync(library);
        await _auditLog.LogLibraryDeletionAsync(UserId, libraryId);

        return NoContent();
    }
}
