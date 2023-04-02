using Microsoft.AspNetCore.Identity;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;

namespace Steamfinity.Cloud.Services;

/// <summary>
/// Initializes the roles in the Steamfinity Cloud authorization system.
/// </summary>
public sealed class RoleInitializer : IRoleInitializer
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleInitializer"/> class.
    /// </summary>
    /// <param name="roleManager">The role manager used to manage roles in the application.</param>
    /// <param name="logger">The logger used to log messages during the role initialization process.</param>
    public RoleInitializer(RoleManager<ApplicationRole> roleManager, ILogger<RoleInitializer> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the roles in the Steamfinity Cloud authorization system. Roles that already exist will be skipped.
    /// Make sure to call this method on application startup.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeRolesAsync()
    {
        await CreateRoleAsync(RoleNames.User);
        await CreateRoleAsync(RoleNames.Administrator);
    }

    /// <summary>
    /// Creates a new role with the specified name if it does not already exist.
    /// </summary>
    /// <param name="roleName">The name of the role to create.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task CreateRoleAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            _logger.LogInformation("The initialization of the '{roleName}' has been skipped because the role already exists.", roleName);
            return;
        }

        var role = new ApplicationRole
        {
            Name = roleName
        };

        var roleCreationResult = await _roleManager.CreateAsync(role);
        if (!roleCreationResult.Succeeded)
        {
            var errorCode = roleCreationResult.Errors.First().Code;

            _logger.LogError("An identity error has occurred while attempting to create the '{roleName}' role.", roleName);
            throw new IdentityException(errorCode);
        }

        _logger.LogInformation("The '{roleName}' role has been successfully created.", roleName);
    }
}
