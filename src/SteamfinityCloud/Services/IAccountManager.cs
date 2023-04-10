using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public interface IAccountManager
{
    IQueryable<Account> Accounts { get; }

    Task<bool> ExistsAsync(Guid accountId);

    Task<Account?> FindByIdAsync(Guid accountId);

    Task<AccountAdditionResult> AddAsync(Account account);

    Task<HashtagsSetResult> SetHashtagsAsync(Account account, IEnumerable<string> hashtags);

    Task UpdateAsync(Account account);

    Task DeleteAsync(Account account);
}
