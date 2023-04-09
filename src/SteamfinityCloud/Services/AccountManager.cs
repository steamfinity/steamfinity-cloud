using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public sealed class AccountManager : IAccountManager
{
    private readonly ApplicationDbContext _context;
    private readonly ILibraryManager _libraryManager;
    private readonly ILimitProvider _limitProvider;
    private readonly ISteamApi _steamApi;

    public AccountManager(
        ApplicationDbContext context,
        ILibraryManager libraryManager,
        ILimitProvider limitProvider,
        ISteamApi steamApi)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _limitProvider = limitProvider ?? throw new ArgumentNullException(nameof(limitProvider));
        _steamApi = steamApi ?? throw new ArgumentNullException(nameof(steamApi));
    }

    public IQueryable<Account> Accounts => _context.Accounts;

    public async Task<Account?> FindByIdAsync(Guid accountId)
    {
        return await _context.Accounts.FindAsync(accountId);
    }

    public async Task<AccountAdditionResult> AddAsync(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        if (!await _libraryManager.ExistsAsync(account.LibraryId))
        {
            return AccountAdditionResult.LibraryNotFound;
        }

        var currentNumberOfAccounts = await _context.Accounts.CountAsync(a => a.LibraryId == account.LibraryId);
        if (currentNumberOfAccounts >= _limitProvider.MaxAccountsPerLibrary)
        {
            return AccountAdditionResult.LibrarySizeExceeded;
        }

        if (!await _steamApi.TryRefreshAccountAsync(account))
        {
            return AccountAdditionResult.InvalidSteamId;
        }

        _ = await _context.Accounts.AddAsync(account);
        _ = await _context.SaveChangesAsync();

        return AccountAdditionResult.Success;
    }

    public async Task<HashtagsSetResult> SetHashtagsAsync(Account account, IEnumerable<string> hashtags)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));
        ArgumentNullException.ThrowIfNull(hashtags, nameof(hashtags));

        if (!await _context.Accounts.AnyAsync(a => a.Id == account.Id))
        {
            return HashtagsSetResult.AccountNotFound;
        }

        if (hashtags.Count() > _limitProvider.MaxHashtagsPerAccount)
        {
            return HashtagsSetResult.HashtagLimitExceeded;
        }

        _context.Hashtags.RemoveRange(_context.Hashtags.Where(h => h.AccountId == account.Id));
        foreach (var hashtag in hashtags.Distinct())
        {
            if (hashtag.Length is < PropertyLengthConstraints.MinHashtagLength or > PropertyLengthConstraints.MaxHashtagLength)
            {
                return HashtagsSetResult.InvalidHashtags;
            }

            _ = await _context.Hashtags.AddAsync(new Hashtag(account.Id, hashtag));
        }

        _ = await _context.SaveChangesAsync();
        return HashtagsSetResult.Success;
    }

    public async Task UpdateAsync(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        _ = _context.Accounts.Update(account);
        _ = await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        _ = _context.Accounts.Attach(account);
        _ = _context.Accounts.Remove(account);

        _ = await _context.SaveChangesAsync();
    }
}
