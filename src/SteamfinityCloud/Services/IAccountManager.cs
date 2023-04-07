using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud.Services;

/// <summary>
/// The service responsible for Steam account management.
/// </summary>
public interface IAccountManager
{
    /// <summary>
    /// Gets all Steam accounts added to Steamfinity.
    /// </summary>
    IQueryable<Account> Accounts { get; }
}
