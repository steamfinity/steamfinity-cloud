using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public sealed class MembershipManager : IMembershipManager
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILibraryManager _libraryManager;
    private readonly ILimitProvider _limitProvider;

    public MembershipManager(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILibraryManager libraryManager,
        ILimitProvider limitProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _libraryManager = libraryManager ?? throw new ArgumentNullException(nameof(libraryManager));
        _limitProvider = limitProvider ?? throw new ArgumentNullException(nameof(limitProvider));
    }

    public IQueryable<Membership> Memberships => _context.Memberships;

    public async Task<Membership?> FindByIdAsync(Guid libraryId, Guid userId)
    {
        return await _context.Memberships.FindAsync(libraryId, userId);
    }

    public async Task<MemberAdditionResult> AddMemberAsync(Guid libraryId, Guid userId, MemberRole role)
    {
        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return MemberAdditionResult.LibraryNotFound;
        }

        if (!await UserExistsAsync(userId))
        {
            return MemberAdditionResult.UserNotFound;
        }

        if (await _context.Memberships.AnyAsync(m => m.LibraryId == libraryId && m.UserId == userId))
        {
            return MemberAdditionResult.MemberAlreadyAdded;
        }

        // Ensure the library has not reached the user limit:
        var currentMemberCount = await _context.Memberships.CountAsync(m => m.LibraryId == libraryId);
        if (currentMemberCount >= _limitProvider.MaxMembersPerLibrary)
        {
            return MemberAdditionResult.MemberLimitExceeded;
        }

        // Ensure the user has not reached the library limit:
        var currentLibrariesCount = await _context.Memberships.CountAsync(m => m.UserId == userId);
        if (currentLibrariesCount >= _limitProvider.MaxLibrariesPerUser)
        {
            return MemberAdditionResult.LibraryLimitExceeded;
        }

        var membership = new Membership
        {
            LibraryId = libraryId,
            UserId = userId
        };

        _ = await _context.Memberships.AddAsync(membership);
        _ = await _context.SaveChangesAsync();

        return MemberAdditionResult.Success;
    }

    public async Task<MemberRoleChangeResult> ChangeMemberRoleAsync(Guid libraryId, Guid userId, MemberRole role)
    {
        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return MemberRoleChangeResult.LibraryNotFound;
        }

        if (!await UserExistsAsync(userId))
        {
            return MemberRoleChangeResult.UserNotFound;
        }

        var membership = await FindByIdAsync(libraryId, userId);
        if (membership == null)
        {
            return MemberRoleChangeResult.UserNotMember;
        }

        membership.Role = role;
        _ = await _context.SaveChangesAsync();

        return MemberRoleChangeResult.Success;
    }

    public async Task<MemberRemovalResult> RemoveMemberAsync(Guid libraryId, Guid userId)
    {
        if (!await _libraryManager.ExistsAsync(libraryId))
        {
            return MemberRemovalResult.LibraryNotFound;
        }

        if (!await UserExistsAsync(userId))
        {
            return MemberRemovalResult.UserNotFound;
        }

        var membership = await FindByIdAsync(libraryId, userId);
        if (membership == null)
        {
            return MemberRemovalResult.UserNotMember;
        }

        _ = _context.Memberships.Remove(membership);
        _ = await _context.SaveChangesAsync();

        return MemberRemovalResult.Success;
    }

    private async Task<bool> UserExistsAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString()) != null;
    }
}
