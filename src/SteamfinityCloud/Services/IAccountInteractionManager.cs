using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public interface IAccountInteractionManager
{
    IQueryable<AccountInteraction> Interactions { get; }

    Task<bool> IsFavoriteAsync(Guid accountId, Guid userId);

    Task<AccountIsFavoriteSetResult> SetIsFavoriteAsync(Guid accountId, Guid userId, bool newIsFavorite);
}
