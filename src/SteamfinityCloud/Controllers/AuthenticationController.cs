using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Steamfinity.Cloud.Controllers;

[ApiController]
[Route("api/authentication")]
public sealed class AuthenticationController : SteamfinityController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    [HttpPost("sign-up")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SignUpAsync(SignUpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var addToAdministratorRole = false;
        if (!string.IsNullOrWhiteSpace(request.AdministratorSignUpKey))
        {
            var correctAdministratorSignUpKey = _configuration["Authentication:AdministratorSignUpKey"];
            if (string.IsNullOrWhiteSpace(correctAdministratorSignUpKey))
            {
                return ApiError(StatusCodes.Status401Unauthorized, "ADMIN_KEY_DISABLED", "The administrator sign-up key has not been configured in the server settings.");
            }

            if (request.AdministratorSignUpKey != correctAdministratorSignUpKey)
            {
                return ApiError(StatusCodes.Status401Unauthorized, "INCORECT_ADMIN_KEY", "The provided administrator sign-up key is incorrect.");
            }

            addToAdministratorRole = true;
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName
        };

        var userCreationResult = await _userManager.CreateAsync(user, request.Password);
        if (!userCreationResult.Succeeded)
        {
            var errorCode = userCreationResult.Errors.First().Code;

            if (errorCode == "DuplicateUserName")
            {
                return ApiError(StatusCodes.Status409Conflict, "DUPLICATE_USERNAME", "There is already a user with this username.");
            }

            if (errorCode == "InvalidUserName")
            {
                return ApiError(StatusCodes.Status400BadRequest, "INVALID_USERNAME", "The provided username is too short, too long, or contains illegal characters.");
            }

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
                "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                return ApiError(StatusCodes.Status400BadRequest, "PASSWORD_TOO_WEAK", "The provided password is too weak.");
            }

            throw new IdentityException(errorCode);
        }

        await AddUserToRoleAsync(user, RoleNames.User);
        if (addToAdministratorRole)
        {
            await AddUserToRoleAsync(user, RoleNames.Administrator);
        }

        return NoContent();
    }

    [HttpPost("sign-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RefreshTokenDetails>> SignInAsync(SignInRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            return ApiError(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "The provided username and password do not match.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (signInResult.IsLockedOut)
        {
            return ApiError(StatusCodes.Status401Unauthorized, "LOCKED_OUT", "This user account has been temporarily blocked due to multiple failed sign-in attempts.");
        }

        if (signInResult.IsNotAllowed)
        {
            return ApiError(StatusCodes.Status401Unauthorized, "NOT_ALLOWED", "You are currently not allowed to sign in.");
        }

        if (!signInResult.Succeeded)
        {
            return ApiError(StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", "The provided username and password do not match.");
        }

        if (user.IsSuspended)
        {
            return ApiError(StatusCodes.Status401Unauthorized, "USER_SUSPENDED", "This user account has been suspended.");
        }

        user.LastSignInTime = DateTimeOffset.UtcNow;
        _ = await _userManager.UpdateAsync(user);

        var refreshToken = await _userManager.GetAuthenticationTokenAsync(user, "Default", "RefreshToken");
        refreshToken ??= await _userManager.GenerateUserTokenAsync(user, "Default", "RefreshToken");

        var tokenDetails = new RefreshTokenDetails
        {
            UserId = user.Id,
            RefreshToken = refreshToken
        };

        return Ok(tokenDetails);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthenticationTokenDetails>> RefreshTokenAsync(TokenRefreshRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return ApiError(StatusCodes.Status401Unauthorized, "INVALID_TOKEN", "The provided authentication token is invalid.");
        }

        if (!await _userManager.VerifyUserTokenAsync(user, "Default", "RefreshToken", request.RefreshToken))
        {
            return ApiError(StatusCodes.Status401Unauthorized, "INVALID_TOKEN", "The provided authentication token is invalid.");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenLifetime = TimeSpan.FromMinutes(30);
        var tokenDetails = new AuthenticationTokenDetails
        {
            Token = CreateJwtBearerAuthenticationToken(claims, tokenLifetime),
            ExpirationTime = DateTimeOffset.UtcNow.Add(tokenLifetime),
            Roles = roles
        };

        return Ok(tokenDetails);
    }

    private async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
    {
        var userAdditionResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!userAdditionResult.Succeeded)
        {
            var errorCode = userAdditionResult.Errors.First().Code;
            throw new IdentityException(errorCode);
        }
    }

    private string CreateJwtBearerAuthenticationToken(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        var issuerSigningKey = _configuration["Authentication:Schemes:Bearer:IssuerSigningKey"];
        if (string.IsNullOrWhiteSpace(issuerSigningKey))
        {
            throw new ConfigurationMissingException("Authentication:Schemes:Bearer:IssuerSigningKey");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime),
            Issuer = _configuration["Authentication:Schemes:Bearer:Issuer"],
            Audience = _configuration["Authentication:Schemes:Bearer:Audience"],
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
