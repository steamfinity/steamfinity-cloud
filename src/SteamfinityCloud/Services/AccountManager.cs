using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Extensions;
using System.Security.Principal;

namespace Steamfinity.Cloud.Services;

public sealed class AccountManager : IAccountManager
{
    private readonly ApplicationDbContext _context;
    private readonly ILimitProvider _limitProvider;
    private readonly ISteamApi _steamApi;

    public AccountManager(ApplicationDbContext context, ILimitProvider limitProvider, ISteamApi steamApi)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _limitProvider = limitProvider ?? throw new ArgumentNullException(nameof(limitProvider));
        _steamApi = steamApi ?? throw new ArgumentNullException(nameof(steamApi));
    }

    public IQueryable<Account> Accounts => _context.Accounts;

    public async Task<bool> ExistsAsync(Guid accountId)
    {
        return await _context.Accounts.AnyAsync(a => a.Id == accountId);
    }

    public async Task<Account?> FindByIdAsync(Guid accountId)
    {
        return await _context.Accounts.FindAsync(accountId);
    }

    public async Task<AccountAdditionResult> AddAsync(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));
        await ThrowIfLibraryNotExistsAsync(account.LibraryId);

        if (await AccountExistsInLibraryAsync(account.LibraryId, account.SteamId))
        {
            return AccountAdditionResult.DuplicateAccount;
        }

        var currentNumberOfAccounts = await GetNumberOfAccountsInLibraryAsync(account.LibraryId);
        if (currentNumberOfAccounts >= _limitProvider.MaxAccountsPerLibrary)
        {
            return AccountAdditionResult.AccountLimitExceeded;
        }

        if (!await _steamApi.TryRefreshAccountAsync(account))
        {
            return AccountAdditionResult.InvalidSteamId;
        }

        _ = await _context.Accounts.AddAsync(account);
        _ = await _context.SaveChangesAsync();

        return AccountAdditionResult.Success;
    }

    public async Task ChangeAccountNameAsync(Account account, string? newAccountName)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.AccountName = newAccountName;
        account.OptimizedAccountName = newAccountName?.OptimizeForSearch();

        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangePasswordAsync(Account account, string? newPassword)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.Password = newPassword;
        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangeAliasAsync(Account account, string? newAlias)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.Alias = newAlias;
        account.OptimizedAlias = newAlias?.OptimizeForSearch();

        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangeColorAsync(Account account, SimpleColor newColor)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.Color = newColor;
        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangePrimeStatusAsync(Account account, bool newPrimeStatus)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.HasPrimeStatus = newPrimeStatus;
        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangeSkillGroupAsync(Account account, SkillGroup newSkillGroup)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.SkillGroup = newSkillGroup;
        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangeCooldownExpirationTimeAsync(Account account, DateTimeOffset newCooldownExpirationTime)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.CooldownExpirationTime = newCooldownExpirationTime;
        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangeLaunchParametersAsync(Account account, string? newLaunchParameters)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.LaunchParameters = newLaunchParameters;
        account.OptimizedLaunchParameters = newLaunchParameters?.OptimizeForSearch();

        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task ChangeNotesAsync(Account account, string? newNotes)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        account.Notes = newNotes;
        account.OptimizedNotes = newNotes?.OptimizeForSearch();

        await UpdateAsync(account, AccountUpdateType.Edit);
    }

    public async Task<HashtagsChangeResult> ChangeHashtagsAsync(Account account, IEnumerable<string> hashtags)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));
        ArgumentNullException.ThrowIfNull(hashtags, nameof(hashtags));

        await ThrowIfAccountNotExistsAsync(account.Id);

        if (hashtags.Count() > _limitProvider.MaxHashtagsPerAccount)
        {
            return HashtagsChangeResult.HashtagLimitExceeded;
        }

        _context.Hashtags.RemoveRange(_context.Hashtags.Where(h => h.AccountId == account.Id));
        foreach (var hashtag in hashtags.Distinct())
        {
            if (hashtag.Length is < PropertyLengthConstraints.MinHashtagLength or > PropertyLengthConstraints.MaxHashtagLength)
            {
                return HashtagsChangeResult.InvalidHashtags;
            }

            _ = await _context.Hashtags.AddAsync(new Hashtag(account.Id, hashtag));
        }

        _ = await _context.SaveChangesAsync();
        return HashtagsChangeResult.Success;
    }

    public async Task<TransferResult> TransferAsync(Account account, Guid newLibraryId)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        await ThrowIfAccountNotExistsAsync(account.Id);
        await ThrowIfLibraryNotExistsAsync(newLibraryId);

        if (await AccountExistsInLibraryAsync(newLibraryId, account.SteamId))
        {
            return TransferResult.DuplicateAccount;
        }

        if (await GetNumberOfAccountsInLibraryAsync(newLibraryId) >= _limitProvider.MaxAccountsPerLibrary)
        {
            return TransferResult.AccountLimitExceeded;
        }

        account.LibraryId = newLibraryId;
        await UpdateAsync(account, AccountUpdateType.Edit);

        return TransferResult.Success;
    }

    public async Task UpdateAsync(Account account, AccountUpdateType updateType = AccountUpdateType.Default)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));
        await ThrowIfAccountNotExistsAsync(account.Id);

        if (updateType == AccountUpdateType.Edit)
        {
            account.LastEditTime = DateTimeOffset.UtcNow;
        }
        else if (updateType == AccountUpdateType.Refresh)
        {
            account.LastRefreshTime = DateTimeOffset.UtcNow;
        }

        _ = await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));
        await ThrowIfAccountNotExistsAsync(account.Id);

        _ = _context.Accounts.Remove(account);
        _ = await _context.SaveChangesAsync();
    }

    private async Task<bool> AccountExistsInLibraryAsync(Guid libraryId, ulong steamId)
    {
        return await _context.Accounts.AnyAsync(a => a.LibraryId == libraryId && a.SteamId == steamId);
    }

    private async Task ThrowIfAccountNotExistsAsync(Guid accountId)
    {
        if (!await ExistsAsync(accountId))
        {
            throw new NotFoundException($"The account '{accountId}' was not found in the database.");
        }
    }

    private async Task ThrowIfLibraryNotExistsAsync(Guid libraryId)
    {
        if (!await _context.Libraries.AnyAsync(l => l.Id == libraryId))
        {
            throw new NotFoundException($"The library '{libraryId}' was not found in the database.");
        }
    }

    private async Task<int> GetNumberOfAccountsInLibraryAsync(Guid libraryId)
    {
        return await _context.Libraries.CountAsync(l => l.Id == libraryId);
    }
}
