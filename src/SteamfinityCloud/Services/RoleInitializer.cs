using Microsoft.AspNetCore.Identity;
using Steamfinity.Cloud.Constants;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Exceptions;

namespace Steamfinity.Cloud.Services;

public sealed class RoleInitializer : IRoleInitializer
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger _logger;

    public RoleInitializer(RoleManager<ApplicationRole> roleManager, ILogger<RoleInitializer> logger)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeRolesAsync()
    {
        await CreateRoleAsync(RoleNames.User);
        await CreateRoleAsync(RoleNames.Administrator);
    }

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
