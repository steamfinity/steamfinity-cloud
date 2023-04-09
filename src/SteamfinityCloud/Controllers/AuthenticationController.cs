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
public sealed class AuthenticationController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public AuthenticationController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthenticationController> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("sign-up")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUpAsync(SignUpRequest request)
    {
        var addToAdministratorRole = false;
        if (!string.IsNullOrWhiteSpace(request.AdministratorSignUpKey))
        {
            var correctAdministratorSignUpKey = _configuration["Authentication:AdministratorSignUpKey"];
            if (string.IsNullOrWhiteSpace(correctAdministratorSignUpKey))
            {
                _logger.LogInformation("The user '{userName}' has attempted to sign up as an administrator, but the administrator sign-up key is not configured.", request.UserName);
                return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "administrator-sign-up-key-not-configured");
            }

            if (request.AdministratorSignUpKey != correctAdministratorSignUpKey)
            {
                _logger.LogInformation("The user '{userName}' has attempted to sign up as an administrator but provided an incorrect administrator sign-up key.", request.UserName);
                return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-administrator-sign-up-key");
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
                _logger.LogInformation("A user has attempted to sign up with a username that already exists: '{userName}'.", request.UserName);
                return Problem(statusCode: StatusCodes.Status409Conflict, detail: "duplicate-user-name");
            }

            if (errorCode == "InvalidUserName")
            {
                _logger.LogInformation("A user has attempted to sign up with an invalid username: '{userName}'.", request.UserName);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "invalid-user-name");
            }

            if (errorCode is "PasswordTooShort" or "PasswordRequiresLower" or "PasswordRequiresUpper" or
                "PasswordRequiresDigit" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars")
            {
                _logger.LogInformation("The user '{userName}' has attempted to sign up with a password that is too weak.", request.UserName);
                return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "password-too-weak");
            }

            _logger.LogError("An identity error has occurred while attempting to create user '{userName}': '{errorCode}'.", request.UserName, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The user '{userId}' has been successfully created with username '{userName}.", user.Id, user.UserName);

        await AddUserToRoleAsync(user, RoleNames.User);
        if (addToAdministratorRole)
        {
            await AddUserToRoleAsync(user, RoleNames.Administrator);
        }

        _logger.LogInformation("The user '{userId}' has successfully signed up with username '{userName}'.", user.Id, user.UserName);
        return NoContent();
    }

    [HttpPost("sign-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenDetails>> SignInAsync(SignInRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            _logger.LogInformation("A non-existent user '{userName}' has attempted to sign in.", request.UserName);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-credentials");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (signInResult.IsLockedOut)
        {
            _logger.LogInformation("A locked-out user '{userId} has attempted to sign in.", request.UserName);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-locked-out");
        }

        if (signInResult.IsNotAllowed)
        {
            _logger.LogInformation("The sign-in attempt of user '{userId}' has been blocked due to incomplete account configuration.", user.Id);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "sign-in-not-allowed");
        }

        if (!signInResult.Succeeded)
        {
            _logger.LogInformation("The user '{userId}' has attempted to sign in but provided an incorrect password.", user.Id);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "incorrect-credentials");
        }

        if (user.IsSuspended)
        {
            _logger.LogInformation("A suspended user '{userId} has attempted to sign in.", request.UserName);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "user-suspended");
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

        _logger.LogInformation("The user '{userId}' has successfully signed in.", user.Id);
        return Ok(tokenDetails);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationTokenDetails>> RefreshTokenAsync(TokenRefreshRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            _logger.LogInformation("A non-existent user '{userId}' has attempted to refresh their authentication token.", request.UserId);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "invalid-token");
        }

        if (!await _userManager.VerifyUserTokenAsync(user, "Default", "RefreshToken", request.RefreshToken))
        {
            _logger.LogInformation("The user '{userId}' has attempted to refresh their authentication token but provided an incorrect refresh token.", user.Id);
            return Problem(statusCode: StatusCodes.Status401Unauthorized, detail: "invalid-token");
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

        _logger.LogInformation("The user '{userId}' has successfully refreshed their authentication token.", user.Id);
        return Ok(tokenDetails);
    }

    private async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
    {
        var userAdditionResult = await _userManager.AddToRoleAsync(user, roleName);
        if (!userAdditionResult.Succeeded)
        {
            var errorCode = userAdditionResult.Errors.First().Code;

            _logger.LogError("An identity error has occurred while attempting to add user '{userId}' to the '{roleName}' role: '{errorCode}'.", user.Id, roleName, errorCode);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The user '{userId}' has been successfully added to the '{roleName}' role.", user.Id, roleName);
    }

    private string CreateJwtBearerAuthenticationToken(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        var issuerSigningKey = _configuration["Authentication:Schemes:Bearer:IssuerSigningKey"];
        if (string.IsNullOrWhiteSpace(issuerSigningKey))
        {
            _logger.LogError("The JWT bearer authentication issuer signing key is not configured in the app settings.");
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
