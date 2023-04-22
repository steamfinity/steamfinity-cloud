using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;

namespace Steamfinity.Cloud.Services;

public sealed class AccountInteractionManager : IAccountInteractionManager
{
    private readonly ApplicationDbContext _context;

    public AccountInteractionManager(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IQueryable<AccountInteraction> Interactions => _context.AccountInteractions;

    private async Task<AccountInteraction?> FindByIdAsync(Guid accountId, Guid userId)
    {
        return await _context.AccountInteractions.FindAsync(accountId, userId);
    }

    public async Task<bool> IsFavoriteAsync(Guid accountId, Guid userId)
    {
        return (await FindByIdAsync(accountId, userId))?.IsFavorite ?? false;
    }

    public async Task SetIsFavoriteAsync(Guid accountId, Guid userId, bool newIsFavorite)
    {
        var interaction = await _context.AccountInteractions.FindAsync(accountId, userId);
        if (interaction == null)
        {
            await ThrowIfAccountNotExistsAsync(accountId);
            await ThrowIfUserNotExistsAsync(userId);

            interaction = new AccountInteraction
            {
                AccountId = accountId,
                UserId = userId
            };

            _ = await _context.AccountInteractions.AddAsync(interaction);
        }

        if (interaction.IsFavorite != newIsFavorite)
        {
            interaction.IsFavorite = newIsFavorite;
            _ = await _context.SaveChangesAsync();
        }
    }

    private async Task ThrowIfUserNotExistsAsync(Guid userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == userId))
        {
            throw new NotFoundException($"The user '{userId}' was not found in the database.");
        }
    }

    private async Task ThrowIfAccountNotExistsAsync(Guid accountId)
    {
        if (!await _context.Accounts.AnyAsync(a => a.Id == accountId))
        {
            throw new NotFoundException($"The user '{accountId}' was not found in the database.");
        }
    }
}
