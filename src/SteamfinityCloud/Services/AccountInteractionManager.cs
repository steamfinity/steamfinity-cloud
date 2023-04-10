using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public sealed class AccountInteractionManager : IAccountInteractionManager
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountManager _accountManager;

    public AccountInteractionManager(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAccountManager accountManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
    }

    public IQueryable<AccountInteraction> Interactions => _context.AccountInteractions;

    public async Task<bool> IsFavoriteAsync(Guid accountId, Guid userId)
    {
        return (await FindByIdAsync(accountId, userId))?.IsFavorite ?? false;
    }

    public async Task<AccountIsFavoriteSetResult> SetIsFavoriteAsync(Guid accountId, Guid userId, bool newIsFavorite)
    {
        var interaction = await _context.AccountInteractions.FindAsync(accountId, userId);
        if (interaction == null)
        {
            if (!await _accountManager.ExistsAsync(accountId))
            {
                return AccountIsFavoriteSetResult.AccountNotFound;
            }

            if (!await _userManager.Users.AnyAsync(u => u.Id == userId))
            {
                return AccountIsFavoriteSetResult.UserNotFound;
            }

            interaction = new AccountInteraction
            {
                AccountId = accountId,
                UserId = userId
            };

            await _context.AccountInteractions.AddAsync(interaction);
        }

        if (interaction.IsFavorite != newIsFavorite)
        {
            interaction.IsFavorite = newIsFavorite;
            await _context.SaveChangesAsync();
        }

        return AccountIsFavoriteSetResult.Success;
    }

    private async Task<AccountInteraction?> FindByIdAsync(Guid accountId, Guid userId)
    {
        return await _context.AccountInteractions.FindAsync(accountId, userId);
    }
}
