using Microsoft.AspNetCore.Mvc;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Models;
using System.Security.Claims;

namespace Steamfinity.Cloud;

[ProducesErrorResponseType(typeof(ApiError))]
public class SteamfinityController : ControllerBase
{
    private Guid? _userId;
    private bool? _isAdministrator;

    public Guid UserId
    {
        get
        {
            if (_userId == null)
            {
                var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new Exception("The user authentication token is missing the NameIdentifier claim.");

                if (!Guid.TryParse(nameIdentifier, out var nameIdentifierGuid))
                {
                    throw new Exception("The user authentication token NameIdentifier claim is not a valid GUID.");
                }

                _userId = nameIdentifierGuid;
            }

            return _userId.Value;
        }
    }

    public bool IsAdministrator
    {
        get
        {
            _isAdministrator ??= User.HasClaim(ClaimTypes.Role, RoleNames.Administrator);
            return _isAdministrator.Value;
        }
    }
}
