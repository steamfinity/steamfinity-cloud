using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;
using Steamfinity.Cloud.Utilities;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(PolicyNames.Users)]
public sealed class AccountsController : SteamfinityController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILibraryManager _libraryManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IAccountManager _accountManager;
    private readonly IAccountInteractionManager _accountInteractionManager;
    private readonly IAuditLog _auditLog;
    private readonly ISteamApi _steamApi;

    public AccountsController(
        UserManager<ApplicationUser> userManager,
        ILibraryManager libraryManager,
        IPermissionManager permissionManager,
        IAccountManager accountManager,
        IAccountInteractionManager accountInteractionManager,
        IAuditLog auditLog,
        ISteamApi steamApi)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _permissionManager = permissionManager ?? throw new ArgumentNullException(nameof(permissionManager));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
        _accountInteractionManager = accountInteractionManager ?? throw new ArgumentNullException(nameof(accountInteractionManager));
        _auditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
        _steamApi = steamApi ?? throw new ArgumentNullException(nameof(steamApi));
    }

    [HttpGet("{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountDetails>> GetAccountAsync(Guid accountId, bool refreshAccount = false)
    {
        var account = await _accountManager.Accounts
                            .Where(a => a.Id == accountId)
                            .Include(a => a.Hashtags)
                            .FirstOrDefaultAsync();

        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (refreshAccount)
        {
            _ = await _steamApi.TryRefreshAccountAsync(account);
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var accountDetails = new AccountDetails
        {
            Id = accountId,
            LibraryId = account.LibraryId,
            SteamId = account.SteamId,
            Alias = account.Alias,
            IsFavorite = await _accountInteractionManager.IsFavoriteAsync(accountId, UserId),
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
            HasPrimeStatus = account.HasPrimeStatus,
            SkillGroup = account.SkillGroup,
            IsCommunityBanned = account.IsCommunityBanned,
            NumberOfVACBans = account.NumberOfVACBans,
            NumberOfGameBans = account.NumberOfGameBans,
            NumberOfDaysSinceLastBan = account.NumberOfDaysSinceLastBan,
            AdditionTime = account.AdditionTime,
            LastEditTime = account.LastEditTime,
            LastUpdateTime = account.LastUpdateTime,
            CooldownExpirationTime = account.CooldownExpirationTime,
            CreationTime = account.CreationTime,
            LastSignOutTime = account.LastSignOutTime,
            LaunchParameters = account.LaunchParameters,
            Notes = account.Notes,
            Hashtags = account.Hashtags.Select(h => h.Name).AsEnumerable()
        };

        return Ok(accountDetails);
    }

    [HttpGet("{accountId}/password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountPasswordDetails>> GetPasswordAsync(Guid accountId, bool logSignIn = false)
    {
        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanViewPasswordsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        if (logSignIn)
        {
            await _auditLog.LogAccountSignInAsync(UserId, account.LibraryId);
        }

        var passwordDetails = new AccountPasswordDetails
        {
            Password = account.Password
        };

        return Ok(passwordDetails);
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
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var previousAccountName = account.AccountName;
        if (request.NewAccountName == previousAccountName)
        {
            return NoContent();
        }

        await _accountManager.ChangeAccountNameAsync(account, request.NewAccountName);
        await _auditLog.LogAccountNameChangeAsync(UserId, account.Id, previousAccountName, account.AccountName);

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
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        await _accountManager.ChangePasswordAsync(account, request.NewPassword);
        await _auditLog.LogAccountPasswordChangeAsync(UserId, account.Id);

        return NoContent();
    }

    [HttpPatch("{accountId}/alias")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeAliasAsync(Guid accountId, AliasChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var previousAlias = account.Alias;
        if (request.NewAlias == previousAlias)
        {
            return NoContent();
        }

        await _accountManager.ChangeAliasAsync(account, request.NewAlias);
        await _auditLog.LogAliasChangeAsync(UserId, account.Id, previousAlias, account.Alias);

        return NoContent();
    }

    [HttpPatch("{accountId}/is-favorite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeIsFavoriteAsync(Guid accountId, IsFavoriteChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (await _userManager.FindByIdAsync(UserId.ToString()) == null)
        {
            return CommonApiErrors.UserNotFoundById;
        }

        await _accountInteractionManager.SetIsFavoriteAsync(account.Id, UserId, request.NewIsFavorite);
        return NoContent();
    }

    [HttpPatch("{accountId}/color")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeColorAsync(Guid accountId, ColorChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        if (request.NewColor == account.Color)
        {
            return NoContent();
        }

        await _accountManager.ChangeColorAsync(account, request.NewColor);
        return NoContent();
    }

    [HttpPatch("{accountId}/has-prime-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeHasPrimeStatusAsync(Guid accountId, HasPrimeStatusChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var previosHasPrimeStatus = account.HasPrimeStatus;
        if (request.NewHasPrimeStatus == previosHasPrimeStatus)
        {
            return NoContent();
        }

        await _accountManager.ChangePrimeStatusAsync(account, request.NewHasPrimeStatus);
        await _auditLog.LogPrimeStatusChangeAsync(UserId, account.Id, previosHasPrimeStatus, account.HasPrimeStatus);

        return NoContent();
    }

    [HttpPatch("{accountId}/skill-group")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeSkillGroupAsync(Guid accountId, SkillGroupChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var previousSkillGroup = account.SkillGroup;
        if (request.NewSkillGroup == previousSkillGroup)
        {
            return NoContent();
        }

        await _accountManager.ChangeSkillGroupAsync(account, request.NewSkillGroup);
        await _auditLog.LogSkillGroupChangeAsync(UserId, account.Id, previousSkillGroup, account.SkillGroup);

        return NoContent();
    }

    [HttpPatch("{accountId}/cooldown-expiration-time")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeCooldownExpirationTimeAsync(Guid accountId, CooldownExpirationTimeChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var previousTime = account.CooldownExpirationTime;
        if (request.NewCooldownExpirationTime == previousTime)
        {
            return NoContent();
        }

        await _accountManager.ChangeCooldownExpirationTimeAsync(account, request.NewCooldownExpirationTime);
        await _auditLog.LogCooldownExpirationTimeChangeAsync(UserId, account.Id, previousTime, account.CooldownExpirationTime);

        return NoContent();
    }

    [HttpPatch("{accountId}/launch-parameters")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeLaunchParametersAsync(Guid accountId, LaunchParametersChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        if (request.NewLaunchParameters == account.LaunchParameters)
        {
            return NoContent();
        }

        await _accountManager.ChangeLaunchParametersAsync(account, request.NewLaunchParameters);
        await _auditLog.LogLaunchParametersChangeAsync(UserId, account.Id);

        return NoContent();
    }

    [HttpPatch("{accountId}/notes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeNotesAsync(Guid accountId, NotesChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        if (request.NewNotes == account.Notes)
        {
            return NoContent();
        }

        await _accountManager.ChangeNotesAsync(account, request.NewNotes);
        await _auditLog.LogNotesChangeAsync(UserId, account.Id);

        return NoContent();
    }

    [HttpPatch("{accountId}/hashtags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeHashtagsAsync(Guid accountId, HashtagsChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        var result = await _accountManager.ChangeHashtagsAsync(account, request.NewHashtags);

        if (result == HashtagsChangeResult.HashtagLimitExceeded)
        {
            return CommonApiErrors.HashtagLimitExceeded;
        }

        if (result == HashtagsChangeResult.InvalidHashtags)
        {
            return CommonApiErrors.InvalidHashtags;
        }

        await _auditLog.LogHashtagsChangeAsync(UserId, account.Id);
        return NoContent();
    }

    [HttpPatch("{accountId}/library")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TransferAccountAsync(Guid accountId, TransferRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        var previousLibraryId = account.LibraryId;

        if (!await _libraryManager.ExistsAsync(previousLibraryId))
        {
            return CommonApiErrors.LibraryNotFound;
        }

        if (!await _libraryManager.ExistsAsync(request.NewLibraryId))
        {
            return CommonApiErrors.LibraryNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(request.NewLibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        if (previousLibraryId == request.NewLibraryId)
        {
            return CommonApiErrors.InvalidTransfer;
        }

        var result = await _accountManager.TransferAsync(account, request.NewLibraryId);
        if (result == TransferResult.AccountLimitExceeded)
        {
            return CommonApiErrors.AccountLimitExceeded;
        }

        await _auditLog.LogAccountTransferAsync(UserId, account.Id, previousLibraryId, account.LibraryId);
        return NoContent();
    }

    [HttpDelete("{accountId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveAccountAsync(Guid accountId)
    {
        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return CommonApiErrors.AccountNotFound;
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return CommonApiErrors.AccessDenied;
        }

        await _accountManager.RemoveAsync(account);
        await _auditLog.LogAccountRemoval(UserId, account.LibraryId, account.Id);

        return NoContent();
    }
}
