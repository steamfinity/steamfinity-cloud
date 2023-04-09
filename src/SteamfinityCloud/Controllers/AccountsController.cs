using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;
using System.Security.Claims;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(PolicyNames.Users)]
public sealed class AccountsController : ControllerBase
{
    private readonly ILibraryManager _libraryManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IAccountManager _accountManager;
    private readonly ISteamApi _steamApi;

    public AccountsController(
        ILibraryManager libraryManager,
        IPermissionManager permissionManager,
        IAccountManager accountManager,
        ISteamApi steamApi)
    {
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _steamApi = steamApi ?? throw new ArgumentNullException(nameof(steamApi));
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

    [HttpGet("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountDetails>> GetAccountAsync(Guid accountId)
    {
        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "account-not-found");
        }

        if (!await _permissionManager.CanViewLibraryAsync(account.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var details = new AccountDetails
        {
            Id = accountId,
            LibraryId = account.LibraryId,
            SteamId = account.SteamId,
            Alias = account.Alias,
            Color = account.Color,
            ProfileName = account.ProfileName,
            RealName = account.RealName,
            AvatarUrl = account.AvatarUrl,
            ProfileUrl = account.ProfileUrl,
            IsProfileSetUp = account.IsProfileSetUp,
            IsProfileVisible = account.IsProfileVisible,
            IsCommentingAllowed = account.IsCommentingAllowed,
            Status = account.Status,
            CurrentGameId = account.CurrentGameId,
            CurrentGameName = account.CurrentGameName,
            IsCommunityBanned = account.IsCommunityBanned,
            NumberOfVACBans = account.NumberOfVACBans,
            NumberOfGameBans = account.NumberOfGameBans,
            NumberOfDaysSinceLastBan = account.NumberOfDaysSinceLastBan,
            LaunchParameters = account.LaunchParameters,
            CreationTime = account.TimeCreated,
            LastSignOutTime = account.TimeSignedOut,
            AdditionTime = account.TimeAdded,
            LastEditTimer = account.TimeEdited,
            LastUpdateTime = account.TimeUpdated,
            Notes = account.Notes
        };

        return Ok(details);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddAccountAsync(AccountAdditionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!await _libraryManager.ExistsAsync(request.LibraryId))
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (!await _permissionManager.CanManageAccountsAsync(request.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        var steamId = await _steamApi.TryResolveSteamIdAsync(request.SteamId);
        if (steamId == null)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-steam-id");
        }

        var account = new Account
        {
            LibraryId = request.LibraryId,
            SteamId = steamId.Value
        };

        var result = await _accountManager.AddAsync(account);

        if (result == AccountAdditionResult.LibraryNotFound)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "library-not-found");
        }

        if (result == AccountAdditionResult.LibrarySizeExceeded)
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "library-size-exceeded");
        }

        if (result == AccountAdditionResult.InvalidSteamId)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-steam-id");
        }

        return NoContent();
    }

    [HttpPatch("{accountId}/alias")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeAliasAsync(Guid accountId, AccountAliasChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "account-not-found");
        }

        if (!await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        account.Alias = request.NewAlias;
        await _accountManager.UpdateAsync(account);

        return NoContent();
    }

    [HttpPatch("{accountId}/color")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeColorAsync(Guid accountId, AccountColorChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "account-not-found");
        }

        if (!await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        account.Color = request.NewColor;
        await _accountManager.UpdateAsync(account);

        return NoContent();
    }

    [HttpPatch("{accountId}/account-name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeAccountNameAsync(Guid accountId, AccountNameChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "account-not-found");
        }

        if (!await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        account.AccountName = request.NewAccountName;
        await _accountManager.UpdateAsync(account);

        return NoContent();
    }

    [HttpPatch("{accountId}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePasswordAsync(Guid accountId, AccountPasswordChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "account-not-found");
        }

        if (!await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        account.Password = request.NewPassword;
        await _accountManager.UpdateAsync(account);

        return NoContent();
    }

    [HttpPatch("{accountId}/notes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeNotesAsync(Guid accountId, AccountNotesChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "account-not-found");
        }

        if (!await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        account.Notes = request.NewNotes;
        await _accountManager.UpdateAsync(account);

        return NoContent();
    }

    [HttpDelete("{accountId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteAccountAsync(Guid accountId)
    {
        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "account-not-found");
        }

        if (!await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return Problem(statusCode: StatusCodes.Status403Forbidden, detail: "access-denied");
        }

        await _accountManager.DeleteAsync(account);
        return NoContent();
    }
}
