using AnyAscii;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Exceptions;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Steamfinity.Cloud.Services;

/// <summary>
/// The service responsible for the communication with the Steam API.
/// </summary>
/// <seealso cref="https://developer.valvesoftware.com/wiki/Steam_Web_API"/>
/// <seealso cref="https://partner.steamgames.com/doc/webapi/ISteamUser"/>
public sealed partial class SteamApi : ISteamApi
{
    private const ulong SteamId64Base = 76561197960265728;

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _steamApiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="SteamApi"/> class.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to make requests to the Steam Web API.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <exception cref="ConfigurationMissingException">Thrown when the Steam API key is not configured in the app settings.</exception>
    public SteamApi(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _steamApiKey = _configuration["SteamApiKey"] ?? throw new ConfigurationMissingException("SteamApiKey");
    }

    /// <summary>
    /// Attempts to resolve the SteamID from the provided <paramref name="input"/> string.
    /// </summary>
    /// <param name="input">The string that might be a SteamID.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the resolved SteamID or <see langword="null"/> in case of failure.</returns>
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
            var steamId = SteamId64Base + (ulong.Parse(steamId2Match.Value[4..]) * 2) + 1;
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
            var steamId = SteamId64Base + ulong.Parse(steamId3Match.Value[4..]);
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
            var steamIdFromSteamid3 = SteamId64Base + number;
            if (await VerifySteamIdAsync(steamIdFromSteamid3))
            {
                return steamIdFromSteamid3;
            }

            var steamIdFromSteamId2 = SteamId64Base + (number * 2) + 1;
            if (await VerifySteamIdAsync(steamIdFromSteamId2))
            {
                return steamIdFromSteamId2;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to refresh the <paramref name="account"/> information with the data provided by the Steam API.
    /// </summary>
    /// <param name="account">The account to refresh</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, returning <see langword="true"/> if the account is refreshed correctly, otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="account"/> is <see langword="null"/>.</exception>
    public async Task<bool> TryRefreshAccountAsync(SteamAccount account)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        var steamIdString = account.SteamId.ToString();

        // Downloads documents in parallel rather than one by one:
        var summaryDocumentTask = GetSummariesDocumentAsync(steamIdString);
        var bansDocumentTask = GetBansDocumentAsync(steamIdString);

        // The Entity Framework Core doesn't support parallel execution, so the documents must be processed individually.
        // This will return false when the provided account has an invalid or non-existent SteamID:

        return TryProcessSummaryDocument(account, await summaryDocumentTask) && TryProcessBansDocument(account, await bansDocumentTask);
    }

    /// <summary>
    /// Refreshes <paramref name="accounts"/> information with the data provided by the Steam API.
    /// </summary>
    /// <param name="accounts">The query providing write access to the accounts.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="accounts"/> are <see langword="null"/>.</exception>
    public async Task RefreshAccountsAsync(IQueryable<SteamAccount> accounts)
    {
        if (accounts == null)
        {
            throw new ArgumentNullException(nameof(accounts));
        }

        // Ignore accounts that have been updated recently to speed up the request and save Steam API quota:

        var steamIds = accounts
                       .Where(account => !account.TimeUpdated.HasValue || DateTimeOffset.UtcNow - account.TimeUpdated.Value > TimeSpan.FromSeconds(10))
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
    }

    /// <summary>
    /// Updates the <paramref name="account"/> information with the data provided in the <paramref name="document"/>.
    /// </summary>
    /// <param name="account">The account to update.</param>
    /// <param name="document">The <see cref="JsonDocument"/> containing <paramref name="account"/> information.</param>
    /// <returns><see langword="true"/> if the information is processed correctly, otherwise <see langword="false"/>.</returns>
    private static bool TryProcessSummaryDocument(SteamAccount account, JsonDocument document)
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

    /// <summary>
    /// Updates general <paramref name="accounts"/> information with the data provided in the <paramref name="document"/>.
    /// </summary>
    /// <param name="accounts">The query providing write access to the accounts.</param>
    /// <param name="document">The <see cref="JsonDocument"/> containing <paramref name="accounts"/> information.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task ProcessSummariesDocumentAsync(IQueryable<SteamAccount> accounts, JsonDocument document)
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

    /// <summary>
    /// Updates the <paramref name="account"/> bans information with the data provided in the <paramref name="document"/>.
    /// </summary>
    /// <param name="account">The account to update.</param>
    /// <param name="document">The <see cref="JsonDocument"/> containing the information about <paramref name="account"/> bans.</param>
    /// <returns><see langword="true"/> if the information is processed correctly, otherwise <see langword="false"/>.</returns>
    private static bool TryProcessBansDocument(SteamAccount account, JsonDocument document)
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

    /// <summary>
    /// Updates <paramref name="accounts"/> bans information with the data provided in the <paramref name="document"/>.
    /// </summary>
    /// <param name="accounts">The query providing write access to the accounts.</param>
    /// <param name="document">The <see cref="JsonDocument"/> containing the information about <paramref name="accounts"/> bans.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
    private static async Task ProcessBansDocumentAsync(IQueryable<SteamAccount> accounts, JsonDocument document)
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

    /// <summary>
    /// Updates the general <paramref name="account"/> information with the data provided in the <paramref name="playerElement"/>
    /// </summary>
    /// <param name="account">The account to update.</param>
    /// <param name="playerElement">The <see cref="JsonElement"/> containing the information.</param>
    /// <exception cref="InvalidOperationException">Thrown when invalid data is returned from the Steam API.</exception>
    private static void UpdatePlayerSummary(SteamAccount account, JsonElement playerElement)
    {
        if (playerElement.TryGetProperty("personaname", out var profileNameElement))
        {
            account.ProfileName = profileNameElement.GetString();
        }
        else
        {
            account.ProfileName = null;
        }

        if (playerElement.TryGetProperty("realname", out var realNameElement))
        {
            account.RealName = realNameElement.GetString();
        }
        else
        {
            account.RealName = null;
        }

        if (playerElement.TryGetProperty("avatarfull", out var avatarUrlElement))
        {
            account.AvatarUrl = avatarUrlElement.GetString();
        }
        else
        {
            account.AvatarUrl = null;
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
        }
        else
        {
            account.CurrentGameName = null;
        }

        if (playerElement.TryGetProperty("timecreated", out var timeCreatedElement))
        {
            account.TimeCreated = DateTimeOffset.FromUnixTimeSeconds(timeCreatedElement.GetInt64());
        }

        if (playerElement.TryGetProperty("lastlogoff", out var timeSignedOutElement))
        {
            account.TimeSignedOut = DateTimeOffset.FromUnixTimeSeconds(timeSignedOutElement.GetInt64());
        }
        else
        {
            account.TimeSignedOut = null;
        }

        account.TimeUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the information about <paramref name="account"/> bans with the data provided in the <paramref name="playerElement"/>.
    /// </summary>
    /// <param name="account">The account to update.</param>
    /// <param name="playerElement">The <see cref="JsonElement"/> containing the information.</param>
    private static void UpdatePlayerBans(SteamAccount account, JsonElement playerElement)
    {
        account.IsCommunityBanned = playerElement.GetProperty("CommunityBanned").GetBoolean();
        account.NumberOfVACBans = playerElement.GetProperty("NumberOfVACBans").GetInt32();
        account.NumberOfGameBans = playerElement.GetProperty("NumberOfGameBans").GetInt32();
        account.NumberOfDaysSinceLastBan = playerElement.GetProperty("DaysSinceLastBan").GetInt32();

        account.TimeUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Attempts to resolve the SteamID from the provided <paramref name="vanityUrl"/>.
    /// </summary>
    /// <remarks>
    /// The Vanity URL is also referred to as "Custom URL".
    /// </remarks>
    /// <param name="vanityUrl">The string that might be a Vanity URL.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the resolved SteamID or <see langword="null"/> in case of failure.</returns>
    /// <seealso cref="https://partner.steamgames.com/doc/webapi/ISteamUser#ResolveVanityURL"/>
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

    /// <summary>
    /// Returns a flag indicating whether the account with the provided <paramref name="steamId"/> figures in the the official Steam database.
    /// </summary>
    /// <remarks>
    /// The <paramref name="steamId"/> must be a SteamID64 [e.g. 76561197960265729].
    /// </remarks>
    /// <param name="steamId">The SteamID to verify.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, returning <see langword="true"/> if the <paramref name="steamId"/> exists, otherwise <see langword="false"/>.</returns>
    private async Task<bool> VerifySteamIdAsync(ulong steamId)
    {
        var response = await _httpClient.GetAsync($"/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={steamId}");
        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync();
        var responseDocument = await JsonDocument.ParseAsync(responseStream);

        return responseDocument.RootElement.GetProperty("response").GetProperty("players").GetArrayLength() == 1;
    }

    /// <summary>
    /// Downloads a <see cref="JsonDocument"/> with the general account information.
    /// </summary>
    /// <param name="steamIds">The comma-separated list of Steam IDs.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the downloaded <see cref="JsonDocument"/>.</returns>
    /// <seealso cref="https://partner.steamgames.com/doc/webapi/ISteamUser#GetPlayerSummaries"/>
    private async Task<JsonDocument> GetSummariesDocumentAsync(string steamIds)
    {
        var response = await _httpClient.GetAsync($"/ISteamUser/GetPlayerSummaries/v2/?key={_steamApiKey}&steamids={steamIds}");
        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonDocument.ParseAsync(responseStream);
    }

    /// <summary>
    /// Downloads a <see cref="JsonDocument"/> with the information about account bans.
    /// </summary>
    /// <param name="steamIds">The comma-separated list of SteamID64s.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the downloaded <see cref="JsonDocument"/>.</returns>
    /// <seealso cref="https://partner.steamgames.com/doc/webapi/ISteamUser#GetPlayerBans"/>
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
