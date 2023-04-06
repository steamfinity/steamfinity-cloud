using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud.Services;

/// <summary>
/// The service responsible for the communication with the Steam API.
/// </summary>
/// <seealso cref="https://developer.valvesoftware.com/wiki/Steam_Web_API"/>
/// <seealso cref="https://partner.steamgames.com/doc/webapi/ISteamUser"/>
public interface ISteamApi
{
    /// <summary>
    /// Attempts to resolve the SteamID from the provided <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The string that might be a SteamID.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the resolved SteamID or <see langword="null"/> in case of failure.</returns>
    Task<ulong?> TryResolveSteamIdAsync(string input);

    /// <summary>
    /// Refreshes the <paramref name="account"/> information with the data provided by the Steam API.
    /// </summary>
    /// <param name="account">The account to refresh</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="account"/> is <see langword="null"/>.</exception>
    Task RefreshAccountAsync(SteamAccount account);

    /// <summary>
    /// Refreshes <paramref name="accounts"/> information with the data provided by the Steam API.
    /// </summary>
    /// <param name="accounts">The query providing write access to the accounts.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accounts"/> are <see langword="null"/>.</exception>
    Task RefreshAccountsAsync(IQueryable<SteamAccount> accounts);
}
