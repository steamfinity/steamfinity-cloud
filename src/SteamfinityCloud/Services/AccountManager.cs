using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud.Services;

/// <summary>
/// The service responsible for Steam account management.
/// </summary>
public sealed class AccountManager : IAccountManager
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountManager"/> class.
    /// </summary>
    /// <param name="context">The application's database context.</param>
    public AccountManager(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets all Steam accounts added to Steamfinity.
    /// </summary>
    public IQueryable<Account> Accounts => _context.Accounts;
}
