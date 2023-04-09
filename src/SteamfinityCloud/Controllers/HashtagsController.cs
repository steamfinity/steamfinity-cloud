using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Services;
using System.Security.Claims;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/hashtags")]
[Authorize(PolicyNames.Users)]
public sealed class HashtagsController : ControllerBase
{
    private readonly IMembershipManager _membershipManager;

    public HashtagsController(IMembershipManager membershipManager)
    {
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
    }

    private Guid UserId
    {
        get
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The user authentication token is missing the NameIdentifier claim.");

            if (!Guid.TryParse(nameIdentifier, out var userId))
            {
                throw new InvalidOperationException("The user authentication token NameIdentifier claim is not a valid GUID.");
            }

            return userId;
        }
    }

    [HttpGet]
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
