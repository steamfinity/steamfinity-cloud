using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public interface IAccountManager
{
    IQueryable<Account> Accounts { get; }

    Task<bool> ExistsAsync(Guid accountId);

    Task<Account?> FindByIdAsync(Guid accountId);

    Task<AccountAdditionResult> AddAsync(Account account);

    Task ChangeAccountNameAsync(Account account, string? newAccountName);

    Task ChangePasswordAsync(Account account, string? newPassword);

    Task ChangeAliasAsync(Account account, string? newAlias);

    Task ChangeColorAsync(Account account, SimpleColor newColor);

    Task ChangePrimeStatusAsync(Account account, bool newPrimeStatus);

    Task ChangeSkillGroupAsync(Account account, SkillGroup newSkillGroup);

    Task ChangeCooldownExpirationTimeAsync(Account account, DateTimeOffset newCooldownExpirationTime);

    Task ChangeLaunchParametersAsync(Account account, string? newLaunchParameters);

    Task ChangeNotesAsync(Account account, string? newNotes);

    Task<HashtagsChangeResult> ChangeHashtagsAsync(Account account, IEnumerable<string> hashtags);

    Task<TransferResult> TransferAsync(Account account, Guid newLibraryId);

    Task UpdateAsync(Account account, AccountUpdateType updateType = AccountUpdateType.Default);

    Task RemoveAsync(Account account);
}
