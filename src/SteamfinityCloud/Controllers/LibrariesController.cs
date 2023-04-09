using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;
using System.Data;
using System.Security.Claims;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/libraries")]
[Authorize(PolicyNames.Users)]
public sealed class LibrariesController : ControllerBase
{
    private readonly ILibraryManager _libraryManager;
    private readonly IMembershipManager _membershipManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IAccountManager _accountManager;

    public LibrariesController(
        ILibraryManager libraryManager,
        IMembershipManager membershipManager,
        IPermissionManager permissionManager,
        IAccountManager accountManager)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
    }

    private Guid UserId
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
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "library-limit-exceeded");
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var result = await _membershipManager.AddMemberAsync(libraryId, request.UserId, request.Role);

        if (result == MemberAdditionResult.LibraryNotFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (result == MemberAdditionResult.UserNotFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        if (result == MemberAdditionResult.MemberAlreadyAdded)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "member-already-added");
        }

        if (result == MemberAdditionResult.MemberLimitExceeded)
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "member-limit-exceeded");
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var result = await _membershipManager.ChangeMemberRoleAsync(libraryId, userId, request.NewRole);

        if (result == MemberRoleChangeResult.LibraryNotFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (result == MemberRoleChangeResult.UserNotFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        if (result == MemberRoleChangeResult.UserNotMember)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-member");
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanManageMembersAsync(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var result = await _membershipManager.RemoveMemberAsync(libraryId, userId);

        if (result == MemberRemovalResult.LibraryNotFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (result == MemberRemovalResult.UserNotFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-found");
        }

        if (result == MemberRemovalResult.UserNotMember)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "user-not-member");
        }

        return NoContent();
    }

    [HttpGet("{libraryId}/accounts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IAsyncEnumerable<AccountOverview>>> GetAccountsAsync(Guid libraryId)
    {
        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanViewLibraryAsync(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var overviews = _accountManager.Accounts
                        .AsNoTracking()
                        .Where(a => a.LibraryId == libraryId)
                        .Select(a => new AccountOverview
                        {
                            Id = a.Id,
                            ProfileName = a.ProfileName,
                            AvatarUrl = a.AvatarUrl
                        })
                        .AsAsyncEnumerable();

        return Ok(overviews);
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanManageLibrary(libraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        await _libraryManager.DeleteAsync(library);
        return NoContent();
    }
}
