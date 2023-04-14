using Steamfinity.Cloud.Models;

namespace Steamfinity.Cloud.Extensions;

public static class QueryPaggingExtension
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PageOptions options)
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        return query.Skip((options.PageNumber - 1) * options.PageSize).Take(options.PageSize);
    }
}
