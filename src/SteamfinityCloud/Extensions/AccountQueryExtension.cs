using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Models;
using System.Text.RegularExpressions;

namespace Steamfinity.Cloud.Extensions;

public static partial class AccountQueryExtension
{
    public static IQueryable<Account> ApplyQueryOptions(this IQueryable<Account> query, AccountQueryOptions options)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        return query.ApplyFilters(options).ApplySorting(options);
    }

    private static IQueryable<Account> ApplyFilters(this IQueryable<Account> query, AccountQueryOptions options)
    {
        if (options.Color != null)
        {
            query = query.Where(a => a.Color == options.Color);
        }

        if (options.SkillGroup != null)
        {
            query = query.Where(a => a.SkillGroup == options.SkillGroup);
        }

        if (options.OnlineVisibility == OnlineAccountVisibility.ShowOnline)
        {
            query = query.Where(a => a.Status != AccountStatus.Offline);
        }
        else if (options.OnlineVisibility == OnlineAccountVisibility.HideOnline)
        {
            query = query.Where(a => a.Status == AccountStatus.Offline);
        }

        if (options.PrimeVisibility == PrimeAccountVisibility.ShowPrime)
        {
            query = query.Where(a => a.HasPrimeStatus);
        }
        else if (options.PrimeVisibility == PrimeAccountVisibility.ShowPrime)
        {
            query = query.Where(a => !a.HasPrimeStatus);
        }

        if (options.BannedVisibility == BannedAccountVisibility.ShowBanned)
        {
            query = query.Where(a => a.IsCommunityBanned.HasValue && a.IsCommunityBanned.Value || a.NumberOfVACBans > 0 || a.NumberOfGameBans > 0);
        }
        else if (options.BannedVisibility == BannedAccountVisibility.HideBanned)
        {
            query = query.Where(a => !(a.IsCommunityBanned.HasValue && a.IsCommunityBanned.Value || a.NumberOfVACBans > 0 || a.NumberOfGameBans > 0));
        }

        if (options.Hashtag != null)
        {
            query = query.Where(a => a.Hashtags.Any(h => h.Name == options.Hashtag));
        }

        if (options.Search != null)
        {
            var optimizedSearch = options.Search.OptimizeForSearch();
            var numberSearch = NumberRegex().Replace(optimizedSearch.Replace("steam_0:1:", "").Replace("[u:1:", ""), "");

            query = query.Where(a =>
            a.SteamId.ToString().Contains(numberSearch) ||
            (a.SteamId - OtherConstants.SteamId64Base).ToString().Contains(numberSearch) ||
            ((a.SteamId - OtherConstants.SteamId64Base - 1) / 2ul).ToString().Contains(numberSearch) ||
            a.OptimizedAccountName != null && a.OptimizedAccountName.Contains(optimizedSearch) ||
            a.OptimizedAlias != null && a.OptimizedAlias.Contains(optimizedSearch) ||
            a.OptimizedProfileName != null && a.OptimizedProfileName.Contains(optimizedSearch) ||
            a.OptimizedRealName != null && a.OptimizedRealName.Contains(optimizedSearch) ||
            a.OptimizedProfileUrl != null && a.OptimizedProfileUrl.Contains(optimizedSearch) ||
            a.OptimizedCurrentGameName != null && a.OptimizedCurrentGameName.Contains(optimizedSearch) ||
            a.OptimizedLaunchParameters != null && a.OptimizedLaunchParameters.Contains(optimizedSearch) ||
            a.OptimizedNotes != null && a.OptimizedNotes.Contains(optimizedSearch) ||
            a.Hashtags.Any(h => h.Name.Contains(optimizedSearch)));
        }

        return query;
    }

    private static IQueryable<Account> ApplySorting(this IQueryable<Account> query, AccountQueryOptions options)
    {
        if (options.AccountSortingCriteria == AccountSortingCriteria.ProfileName)
        {
            if (options.AccountSortingOrder == AccountSortingOrder.Descending)
            {
                query = query.OrderByDescending(a => a.ProfileName);
            }
            else
            {
                query = query.OrderBy(a => a.ProfileName);
            }
        }
        else if (options.AccountSortingCriteria == AccountSortingCriteria.CreationTime)
        {
            if (options.AccountSortingOrder == AccountSortingOrder.Descending)
            {
                query = query.OrderByDescending(a => a.CreationTime);
            }
            else
            {
                query = query.OrderBy(a => a.CreationTime);
            }
        }
        else if (options.AccountSortingCriteria == AccountSortingCriteria.AdditionTime)
        {
            if (options.AccountSortingOrder == AccountSortingOrder.Descending)
            {
                query = query.OrderByDescending(a => a.AdditionTime);
            }
            else
            {
                query = query.OrderBy(a => a.AdditionTime);
            }
        }
        else if (options.AccountSortingCriteria == AccountSortingCriteria.AdditionTime)
        {
            if (options.AccountSortingOrder == AccountSortingOrder.Descending)
            {
                query = query.OrderByDescending(a => a.AdditionTime);
            }
            else
            {
                query = query.OrderBy(a => a.AdditionTime);
            }
        }
        else if (options.AccountSortingCriteria == AccountSortingCriteria.LastSignOutTime)
        {
            if (options.AccountSortingOrder == AccountSortingOrder.Descending)
            {
                query = query.OrderByDescending(a => a.LastSignOutTime);
            }
            else
            {
                query = query.OrderBy(a => a.LastSignOutTime);
            }
        }
        else if (options.AccountSortingCriteria == AccountSortingCriteria.CompetitiveCooldownExpirationTime)
        {
            if (options.AccountSortingOrder == AccountSortingOrder.Descending)
            {
                query = query.OrderByDescending(a => a.CompetitiveCooldownExpirationTime);
            }
            else
            {
                query = query.OrderBy(a => a.CompetitiveCooldownExpirationTime);
            }
        }
        else if (options.AccountSortingCriteria == AccountSortingCriteria.SkillGroup)
        {
            if (options.AccountSortingOrder == AccountSortingOrder.Descending)
            {
                query = query.OrderByDescending(a => a.SkillGroup);
            }
            else
            {
                query = query.OrderBy(a => a.SkillGroup);
            }
        }

        return query;
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex NumberRegex();
}
