using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;
using Steamfinity.Cloud.Models;
using Steamfinity.Cloud.Services;
using Steamfinity.Cloud.Utilities;
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
    private readonly IAuditLog _auditLog;

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IAuditLog auditLog)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _auditLog = auditLog ?? throw new ArgumentNullException(nameof(auditLog));
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
                return CommonApiErrors.AdministratorSignUpKeyNotConfigured;
            }

            if (request.AdministratorSignUpKey != correctAdministratorSignUpKey)
            {
                return CommonApiErrors.IncorrectAdministratorSignUpKey;
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
                return CommonApiErrors.DuplicateUserName;
            }

            if (errorCode == "InvalidUserName")
            {
                return CommonApiErrors.InvalidUserName;
            }

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
                "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                return CommonApiErrors.PasswordTooWeak;
            }

            throw new IdentityException(errorCode);
        }

        await AddUserToRoleAsync(user, RoleNames.User);
        if (addToAdministratorRole)
        {
            await AddUserToRoleAsync(user, RoleNames.Administrator);
        }

        await _auditLog.LogUserSignUpAsync(user.Id);
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
            return CommonApiErrors.InvalidCredentials;
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (signInResult.IsLockedOut)
        {
            return CommonApiErrors.UserLockedOut;
        }

        if (signInResult.IsNotAllowed)
        {
            return CommonApiErrors.SignInNotAllowed;
        }

        if (!signInResult.Succeeded)
        {
            return CommonApiErrors.InvalidCredentials;
        }

        if (user.IsSuspended)
        {
            return CommonApiErrors.UserSuspended;
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

        await _auditLog.LogUserSignInAsync(user.Id);
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
            return CommonApiErrors.InvalidToken;
        }

        if (!await _userManager.VerifyUserTokenAsync(user, "Default", "RefreshToken", request.RefreshToken))
        {
            return CommonApiErrors.InvalidToken;
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

        await _auditLog.LogTokenRefreshAsync(user.Id);
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
