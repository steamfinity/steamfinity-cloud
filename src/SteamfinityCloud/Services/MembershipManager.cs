using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Exceptions;

namespace Steamfinity.Cloud.Services;

public sealed class MembershipManager : IMembershipManager
{
    private readonly ApplicationDbContext _context;
    private readonly ILimitProvider _limitProvider;

    public MembershipManager(ApplicationDbContext context, ILimitProvider limitProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _limitProvider = limitProvider ?? throw new ArgumentNullException(nameof(limitProvider));
    }

    public IQueryable<Membership> Memberships => _context.Memberships;

    public async Task<bool> ExistsAsync(Guid libraryId, Guid userId)
    {
        return await _context.Memberships.AnyAsync(m => m.LibraryId == libraryId && m.UserId == userId);
    }

    public async Task<Membership?> FindByIdAsync(Guid libraryId, Guid userId)
    {
        return await _context.Memberships.FindAsync(libraryId, userId);
    }

    public async Task<MemberAdditionResult> AddMemberAsync(Guid libraryId, Guid userId, MemberRole role)
    {
        await ThrowIfLibraryNotExistsAsync(libraryId);
        await ThrowIfUserNotExistsAsync(userId);

        if (await _context.Memberships.AnyAsync(m => m.LibraryId == libraryId && m.UserId == userId))
        {
            return MemberAdditionResult.Success;
        }

        var currentLibrariesCount = await _context.Memberships.CountAsync(m => m.UserId == userId);
        if (currentLibrariesCount >= _limitProvider.MaxLibrariesPerUser)
        {
            return MemberAdditionResult.LibraryLimitExceeded;
        }

        var currentMemberCount = await _context.Memberships.CountAsync(m => m.LibraryId == libraryId);
        if (currentMemberCount >= _limitProvider.MaxMembersPerLibrary)
        {
            return MemberAdditionResult.MemberLimitExceeded;
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
        await ThrowIfLibraryNotExistsAsync(libraryId);
        await ThrowIfUserNotExistsAsync(userId);

        var membership = await FindByIdAsync(libraryId, userId);
        if (membership == null)
        {
            return MemberRoleChangeResult.UserNotMember;
        }

        membership.Role = role;
        _ = await _context.SaveChangesAsync();

        return MemberRoleChangeResult.Success;
    }

    public async Task RemoveMemberAsync(Guid libraryId, Guid userId)
    {
        await ThrowIfLibraryNotExistsAsync(libraryId);
        await ThrowIfUserNotExistsAsync(userId);

        var membership = await FindByIdAsync(libraryId, userId);
        if (membership != null)
        {
            _ = _context.Memberships.Remove(membership);
            _ = await _context.SaveChangesAsync();
        }
    }

    private async Task ThrowIfUserNotExistsAsync(Guid userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == userId))
        {
            throw new NotFoundException($"The user '{userId}' was not found in the database.");
        }
    }

    private async Task ThrowIfLibraryNotExistsAsync(Guid libraryId)
    {
        if (!await _context.Libraries.AnyAsync(l => l.Id == libraryId))
        {
            throw new NotFoundException($"The library '{libraryId}' was not found in the database.");
        }
    }
}
