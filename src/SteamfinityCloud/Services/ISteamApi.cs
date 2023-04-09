using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud.Services;

public interface ISteamApi
{
    Task<ulong?> TryResolveSteamIdAsync(string input);

    Task<bool> TryRefreshAccountAsync(Account account);

    Task RefreshAccountsAsync(IQueryable<Account> accounts);
}
