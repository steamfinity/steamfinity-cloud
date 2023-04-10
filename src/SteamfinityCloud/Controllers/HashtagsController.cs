using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Services;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/hashtags")]
[Authorize(PolicyNames.Users)]
public sealed class HashtagsController : SteamfinityController
{
    private readonly IMembershipManager _membershipManager;

    public HashtagsController(IMembershipManager membershipManager)
    {
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IAsyncEnumerable<string>> GetAllHashtagsAsync()
    {
        var hashtags = _membershipManager.Memberships
            .AsNoTracking()
            .Where(m => m.UserId == UserId)
            .Include(m => m.Library)
            .ThenInclude(l => l.Accounts)
            .ThenInclude(a => a.Hashtags)
            .SelectMany(m => m.Library.Accounts.SelectMany(a => a.Hashtags))
            .Select(h => h.Name)
            .Distinct()
            .AsAsyncEnumerable();

        return Ok(hashtags);
    }
}
