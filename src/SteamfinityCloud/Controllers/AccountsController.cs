﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Extensions;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize(PolicyNames.Users)]
public sealed class AccountsController : SteamfinityController
{
    private readonly IPermissionManager _permissionManager;
    private readonly IAccountManager _accountManager;
    private readonly IAccountInteractionManager _accountInteractionManager;
    private readonly IAuditLog _auditLog;
    private readonly ISteamApi _steamApi;

    public AccountsController(
        IPermissionManager permissionManager,
        IAccountManager accountManager,
        IAccountInteractionManager accountInteractionManager,
        IAuditLog auditLog,
        ISteamApi steamApi)
    {
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
            return AccountNotFoundError();
        }

        if (refreshAccount)
        {
            _ = await _steamApi.TryRefreshAccountAsync(account);
        }

        if (!IsAdministrator && !await _permissionManager.CanViewLibraryAsync(account.LibraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not a member of the library that holds the account.");
        }

        var details = new AccountDetails
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

        return Ok(details);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanViewPasswordsAsync(account.LibraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to view account passwords in this library.");
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        var previousAccountName = account.AccountName;
        if (request.NewAccountName == previousAccountName)
        {
            return NoContent();
        }

        account.AccountName = request.NewAccountName;
        account.OptimizedAccountName = request.NewAccountName?.OptimizeForSearch();
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        account.Password = request.NewPassword;
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        var previousAlias = account.Alias;
        if (request.NewAlias == previousAlias)
        {
            return NoContent();
        }

        account.Alias = request.NewAlias;
        account.OptimizedAlias = request.NewAlias?.OptimizeForSearch();
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        var result = await _accountInteractionManager.SetIsFavoriteAsync(account.Id, UserId, request.NewIsFavorite);

        if (result == AccountIsFavoriteSetResult.AccountNotFound)
        {
            return AccountNotFoundError();
        }

        if (result == AccountIsFavoriteSetResult.UserNotFound)
        {
            return ApiError(StatusCodes.Status404NotFound, "USER_NOT_FOUND", "There is no user with this identifier.");
        }

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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        if (request.NewColor == account.Color)
        {
            return NoContent();
        }

        account.Color = request.NewColor;
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);

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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        var previosHasPrimeStatus = account.HasPrimeStatus;
        if (request.NewHasPrimeStatus == previosHasPrimeStatus)
        {
            return NoContent();
        }

        account.HasPrimeStatus = request.NewHasPrimeStatus;
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        var previousSkillGroup = account.SkillGroup;
        if (request.NewSkillGroup == previousSkillGroup)
        {
            return NoContent();
        }

        account.SkillGroup = request.NewSkillGroup;
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        var previousTime = account.CooldownExpirationTime;
        if (request.NewCooldownExpirationTime == previousTime)
        {
            return NoContent();
        }

        account.CooldownExpirationTime = request.NewCooldownExpirationTime;
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        if (request.NewLaunchParameters == account.LaunchParameters)
        {
            return NoContent();
        }

        account.LaunchParameters = request.NewLaunchParameters;
        account.OptimizedLaunchParameters = request.NewLaunchParameters?.OptimizeForSearch();
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        if (request.NewNotes == account.Notes)
        {
            return NoContent();
        }

        account.Notes = request.NewNotes;
        account.OptimizedNotes = request.NewNotes?.OptimizeForSearch();
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
    public async Task<IActionResult> SetHashtagsAsync(Guid accountId, HashtagsSetRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var account = await _accountManager.FindByIdAsync(accountId);
        if (account == null)
        {
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage accounts in this library.");
        }

        var result = await _accountManager.SetHashtagsAsync(account, request.NewHashtags);

        if (result == HashtagsSetResult.AccountNotFound)
        {
            return AccountNotFoundError();
        }

        if (result == HashtagsSetResult.HashtagLimitExceeded)
        {
            return ApiError(StatusCodes.Status403Forbidden, "HASHTAG_LIMIT_EXCEEDED", "You are not allowed to add this number of hashtags.");
        }

        if (result == HashtagsSetResult.InvalidHashtags)
        {
            return ApiError(StatusCodes.Status400BadRequest, "INVALID_HASHTAGS", "At least one of the provided hashtags is invalid.");
        }

        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        var previousLibraryId = account.LibraryId;

        // Ensure the user has permission to manage accounts in the current library:
        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to transfer accounts from this library.");
        }

        // Ensure the user has permission to manage accounts in the new library:
        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(request.NewLibraryId, UserId))
        {
            return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to transfer accounts to this library.");
        }

        // Prevent the account from being moved to the same library it is currently in:
        if (account.LibraryId == request.NewLibraryId)
        {
            return ApiError(StatusCodes.Status400BadRequest, "INVALID_OPERATION", "You cannot transfer an account to the same library that it is currently in.");
        }

        account.LibraryId = request.NewLibraryId;
        account.LastEditTime = DateTimeOffset.UtcNow;

        await _accountManager.UpdateAsync(account);
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
            return AccountNotFoundError();
        }

        if (!IsAdministrator && !await _permissionManager.CanManageAccountsAsync(account.LibraryId, UserId))
        {
            return NoAccountManagementPermissionsError();
        }

        await _accountManager.RemoveAsync(account);
        await _auditLog.LogAccountRemoval(UserId, account.LibraryId, account.Id);

        return NoContent();
    }

    private static ObjectResult AccountNotFoundError()
    {
        return ApiError(StatusCodes.Status404NotFound, "ACCOUNT_NOT_FOUND", "There is no account with this identifier.");
    }

    private static ObjectResult NoAccountManagementPermissionsError()
    {
        return ApiError(StatusCodes.Status403Forbidden, "ACCESS_DENIED", "You are not allowed to manage accounts in this library.");
    }
}
