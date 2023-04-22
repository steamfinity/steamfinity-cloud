using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud.Services;

public interface IAccountInteractionManager
{
    IQueryable<AccountInteraction> Interactions { get; }

    Task<bool> IsFavoriteAsync(Guid accountId, Guid userId);

    Task SetIsFavoriteAsync(Guid accountId, Guid userId, bool newIsFavorite);
}
