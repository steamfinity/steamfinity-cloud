using AnyAscii;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Extensions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Steamfinity.Cloud.Services;

public sealed partial class SteamApi : ISteamApi
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _steamApiKey;

    public SteamApi(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _steamApiKey = _configuration["SteamApiKey"] ?? throw new ConfigurationMissingException("SteamApiKey");
    }

    public async Task<ulong?> TryResolveSteamIdAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // Attempt to match SteamID2 (e.g. 'STEAM_0:1:123456789'):
        var steamId2Match = SteamId2Regex().Match(input);
        if (steamId2Match.Success)
        {
            var steamId = OtherConstants.SteamId64Base + (ulong.Parse(steamId2Match.Value[4..]) * 2) + 1;
            if (await VerifySteamIdAsync(steamId))
            {
                return steamId;
            }

            return null;
        }

        // Attempt to match SteamID3 (e.g. '[U:1:123456789]'):
        var steamId3Match = SteamId3Regex().Match(input);
        if (steamId3Match.Success)
        {
            var steamId = OtherConstants.SteamId64Base + ulong.Parse(steamId3Match.Value[4..]);
            if (await VerifySteamIdAsync(steamId))
            {
                return steamId;
            }

            return null;
        }

        // Attempt to match SteamID64 (e.g. '76561197960265729'):
        var steamId64Match = SteamId64Regex().Match(input);
        if (steamId64Match.Success)
        {
            var steamId = ulong.Parse(steamId64Match.Value);
            if (await VerifySteamIdAsync(steamId))
            {
                return steamId;
            }

            return null;
        }

        // Attempt to match vanity URL (e.g. 'https://steamcommunity.com/id/XXXXXXXXXX/'):
        var vanityUrlMatch = VanityUrlRegex().Match(input);
        if (vanityUrlMatch.Success)
        {
            var steamIdFromFullVanityUrl = await TryResolveVanityUrlAsync(vanityUrlMatch.Value[3..]);
            if (steamIdFromFullVanityUrl.HasValue)
            {
                return steamIdFromFullVanityUrl;
            }
        }

        // Attempt to resolve a non-standard Steam ID using experimental methods.
        // Clean up the input string before starting:

        var normalizedInput = input
                             .Replace(" ", string.Empty)
                             .Replace('\\', '/')
                             .ToLowerInvariant()
                             .Normalize()
                             .Transliterate();

        var filteredInput = AlphanumericRegex().Replace(normalizedInput, string.Empty);

        // After removing all special characters, try to resolve the vanity URL once again:
        var steamIdFromPartialVanityUrl = await TryResolveVanityUrlAsync(filteredInput);
        if (steamIdFromPartialVanityUrl.HasValue)
        {
            return steamIdFromPartialVanityUrl;
        }

        // If all else fails, try to resolve SteamID2 from the first number found in the string.
        // WARNING: This may return an incorrect account.

        var numberMatch = NumberRegex().Match(filteredInput);
        if (numberMatch.Success && ulong.TryParse(numberMatch.Value, out var number))
        {
            var steamIdFromSteamid3 = OtherConstants.SteamId64Base + number;
            if (await VerifySteamIdAsync(steamIdFromSteamid3))
            {
                return steamIdFromSteamid3;
            }

            var steamIdFromSteamId2 = OtherConstants.SteamId64Base + (number * 2) + 1;
            if (await VerifySteamIdAsync(steamIdFromSteamId2))
            {
                return steamIdFromSteamId2;
            }
        }

        return null;
    }

    public async Task<bool> TryRefreshAccountAsync(Account account)
    {
        ArgumentNullException.ThrowIfNull(account, nameof(account));

        var steamIdString = account.SteamId.ToString();

        // Downloads documents in parallel rather than one by one:
        var summaryDocumentTask = GetSummariesDocumentAsync(steamIdString);
        var bansDocumentTask = GetBansDocumentAsync(steamIdString);

        // The Entity Framework Core doesn't support parallel execution, so the documents must be processed individually.
        // This will return false when the provided account has an invalid or non-existent SteamID:

        return TryProcessSummaryDocument(account, await summaryDocumentTask) && TryProcessBansDocument(account, await bansDocumentTask);
    }

    public async Task RefreshAccountsAsync(IQueryable<Account> accounts)
    {
        ArgumentNullException.ThrowIfNull(accounts, nameof(accounts));

        // Ignore accounts that have been updated recently to speed up the request and save Steam API quota:
        var steamIds = accounts
                       .Where(account => !account.LastUpdateTime.HasValue || DateTimeOffset.UtcNow - account.LastUpdateTime.Value > TimeSpan.FromSeconds(10))
                       .Select(account => account.SteamId)
                       .AsAsyncEnumerable();

        // The Steam API supports up to 100 accounts per request.
        // The following code splits them into groups and executes parallel requests:

        var groupSize = 0;
        var groupBuilder = new StringBuilder();

        var summariesDocumentTasks = new List<Task<JsonDocument>>();
        var bansDocumentTasks = new List<Task<JsonDocument>>();

        await foreach (var steamId in steamIds)
        {
            _ = groupBuilder.Append($"{steamId},");
            ++groupSize;

            if (groupSize == 100)
            {
                var group = groupBuilder.ToString();

                summariesDocumentTasks.Add(GetSummariesDocumentAsync(group));
                bansDocumentTasks.Add(GetBansDocumentAsync(group));

                groupSize = 0;
                _ = groupBuilder.Clear();
            }
        }

        if (groupSize > 0)
        {
            var group = groupBuilder.ToString();

            summariesDocumentTasks.Add(GetSummariesDocumentAsync(group));
            bansDocumentTasks.Add(GetBansDocumentAsync(group));
        }

        // Entity Framework Core doesn't support parallel operations.
        // Downloaded JSON documents must be parsed one by one:

        foreach (var summariesDocumentTask in summariesDocumentTasks)
        {
            await ProcessSummariesDocumentAsync(accounts, await summariesDocumentTask);
        }

        foreach (var bansDocumentTask in bansDocumentTasks)
        {
            await ProcessBansDocumentAsync(accounts, await bansDocumentTask);
        }

        await _context.SaveChangesAsync();
    }

    private static bool TryProcessSummaryDocument(Account account, JsonDocument document)
    {
        var playersElement = document.RootElement.GetProperty("response").GetProperty("players");

        // The array length will be zero if an incorrect/non-existent SteamID is provided:
        if (playersElement.GetArrayLength() == 0)
        {
            return false;
        }

        UpdatePlayerSummary(account, playersElement[0]);
        return true;
    }

    private static async Task ProcessSummariesDocumentAsync(IQueryable<Account> accounts, JsonDocument document)
    {
        var playersElement = document.RootElement.GetProperty("response").GetProperty("players");
        foreach (var playerElement in playersElement.EnumerateArray())
        {
            var steamId = ulong.Parse(playerElement.GetProperty("steamid").GetString()!);
            var account = await accounts.FirstOrDefaultAsync(predicate => predicate.SteamId == steamId);

            // The account may have been deleted when the document was downloaded.
            // Perform a null check to ensure it still exists:

            if (account != null)
            {
                UpdatePlayerSummary(account, playerElement);
            }
        }
    }

    private static bool TryProcessBansDocument(Account account, JsonDocument document)
    {
        var playersElement = document.RootElement.GetProperty("players");

        // The array length will be zero if an incorrect/non-existent SteamID is provided:
        if (playersElement.GetArrayLength() == 0)
        {
            return false;
        }

        UpdatePlayerBans(account, playersElement[0]);
        return true;
    }

    private static async Task ProcessBansDocumentAsync(IQueryable<Account> accounts, JsonDocument document)
    {
        var playersElement = document.RootElement.GetProperty("players");
        foreach (var playerElement in playersElement.EnumerateArray())
        {
            var steamId = ulong.Parse(playerElement.GetProperty("SteamId").GetString()!);
            var account = await accounts.FirstOrDefaultAsync(predicate => predicate.SteamId == steamId);

            // The account may have been deleted when the document was downloaded.
            // Perform a null check to ensure it still exists:

            if (account != null)
            {
                UpdatePlayerBans(account, playerElement);
            }
        }
    }

    private static void UpdatePlayerSummary(Account account, JsonElement playerElement)
    {
        if (playerElement.TryGetProperty("personaname", out var profileNameElement))
        {
            account.ProfileName = profileNameElement.GetString();
            account.OptimizedProfileName = account.ProfileName?.OptimizeForSearch();
        }
        else
        {
            account.ProfileName = null;
            account.OptimizedProfileName = null;
        }

        if (playerElement.TryGetProperty("realname", out var realNameElement))
        {
            account.RealName = realNameElement.GetString();
            account.OptimizedRealName = account.RealName?.OptimizeForSearch();
        }
        else
        {
            account.RealName = null;
            account.OptimizedRealName = null;
        }

        if (playerElement.TryGetProperty("avatarfull", out var avatarUrlElement))
        {
            account.AvatarUrl = avatarUrlElement.GetString();
        }
        else
        {
            account.AvatarUrl = null;
        }

        if (playerElement.TryGetProperty("profileurl", out var profileUrlProperty))
        {
            account.ProfileUrl = profileUrlProperty.GetString();
            account.OptimizedProfileUrl = account.ProfileUrl?.OptimizeForSearch();
        }
        else
        {
            account.ProfileUrl = null;
            account.OptimizedProfileUrl = null;
        }

        if (playerElement.TryGetProperty("profilestate", out var isProfileSetUpElement))
        {
            account.IsProfileSetUp = isProfileSetUpElement.GetInt32() == 1;
        }
        else
        {
            account.IsProfileSetUp = false;
        }

        if (playerElement.TryGetProperty("communityvisibilitystate", out var isProfileVisibleElement))
        {
            account.IsProfileVisible = isProfileVisibleElement.GetInt32() == 3;
        }
        else
        {
            account.IsProfileVisible = false;
        }

        if (playerElement.TryGetProperty("commentpermission", out var isCommentingAllowedElement))
        {
            account.IsCommentingAllowed = isCommentingAllowedElement.GetInt32() == 1;
        }
        else
        {
            account.IsCommentingAllowed = false;
        }

        if (playerElement.TryGetProperty("personastate", out var statusElement))
        {
            var statusInteger = statusElement.GetInt32();
            if (statusInteger is < 0 or > 6)
            {
                throw new InvalidOperationException("The account status is outside of the valid range (0 to 6).");
            }

            account.Status = (AccountStatus)statusInteger;
        }
        else
        {
            account.Status = null;
        }

        if (playerElement.TryGetProperty("gameid", out var currentGameIdElement))
        {
            var currentGameIdString = currentGameIdElement.GetString();
            if (currentGameIdString != null)
            {
                account.CurrentGameId = ulong.Parse(currentGameIdString);
            }
            else
            {
                account.CurrentGameId = null;
            }
        }
        else
        {
            account.CurrentGameId = null;
        }

        if (playerElement.TryGetProperty("gameextrainfo", out var currentGameNameElement))
        {
            account.CurrentGameName = currentGameNameElement.GetString();
            account.OptimizedCurrentGameName = account.CurrentGameName?.OptimizeForSearch();
        }
        else
        {
            account.CurrentGameName = null;
            account.OptimizedCurrentGameName = null;
        }

        if (playerElement.TryGetProperty("timecreated", out var creationTimeElement))
        {
            account.CreationTime = DateTimeOffset.FromUnixTimeSeconds(creationTimeElement.GetInt64());
        }

        if (playerElement.TryGetProperty("lastlogoff", out var lastSignOutTimeElement))
        {
            account.LastSignOutTime = DateTimeOffset.FromUnixTimeSeconds(lastSignOutTimeElement.GetInt64());
        }
        else
        {
            account.LastSignOutTime = null;
        }

        account.LastUpdateTime = DateTimeOffset.UtcNow;
    }

    private static void UpdatePlayerBans(Account account, JsonElement playerElement)
    {
        account.IsCommunityBanned = playerElement.GetProperty("CommunityBanned").GetBoolean();
        account.NumberOfVACBans = playerElement.GetProperty("NumberOfVACBans").GetInt32();
        account.NumberOfGameBans = playerElement.GetProperty("NumberOfGameBans").GetInt32();
        account.NumberOfDaysSinceLastBan = playerElement.GetProperty("DaysSinceLastBan").GetInt32();

        account.LastUpdateTime = DateTimeOffset.UtcNow;
    }

    private async Task<ulong?> TryResolveVanityUrlAsync(string vanityUrl)
    {
        var response = await _httpClient.GetAsync($"/ISteamUser/ResolveVanityURL/v1/?key={_steamApiKey}&vanityurl={vanityUrl}");
        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync();
        var responseDocument = await JsonDocument.ParseAsync(responseStream);
        var responseElement = responseDocument.RootElement.GetProperty("response");

        if (responseElement.GetProperty("success").GetInt32() != 1)
        {
            return null;
        }

        var steamId = responseElement.GetProperty("steamid").GetString()!;
        return ulong.Parse(steamId);
    }

    private async Task<bool> VerifySteamIdAsync(ulong steamId)
    {
        var response = await _httpClient.GetAsync($"/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={steamId}");
        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync();
        var responseDocument = await JsonDocument.ParseAsync(responseStream);

        return responseDocument.RootElement.GetProperty("response").GetProperty("players").GetArrayLength() == 1;
    }

    private async Task<JsonDocument> GetSummariesDocumentAsync(string steamIds)
    {
        var response = await _httpClient.GetAsync($"/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={steamIds}");
        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(responseStream);
    }

    private async Task<JsonDocument> GetBansDocumentAsync(string steamIds)
    {
        var response = await _httpClient.GetAsync($"/ISteamUser/GetPlayerBans/v1/?key={_steamApiKey}&steamids={steamIds}");
        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(responseStream);
    }

    [GeneratedRegex("\\d+")]
    private static partial Regex NumberRegex();

    [GeneratedRegex("[^a-zA-Z0-9]+")]
    private static partial Regex AlphanumericRegex();

    [GeneratedRegex("0:1:\\d+")]
    private static partial Regex SteamId2Regex();

    [GeneratedRegex("u:1:\\d+")]
    private static partial Regex SteamId3Regex();

    [GeneratedRegex("7\\d{16}")]
    private static partial Regex SteamId64Regex();

    [GeneratedRegex("id/\\w+")]
    private static partial Regex VanityUrlRegex();
}
