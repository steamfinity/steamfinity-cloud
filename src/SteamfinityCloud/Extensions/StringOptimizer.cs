using AnyAscii;

namespace Steamfinity.Cloud.Extensions;

public static class StringOptimizer
{
    public static string OptimizeForSearch(this string value)
    {
        return value
        .ToLowerInvariant()
        .Replace(" ", string.Empty)
        .Transliterate()
        .ToLowerInvariant()
        .ReplaceLineEndings();
    }
}
