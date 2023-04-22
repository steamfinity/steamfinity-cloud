using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;
using Steamfinity.Cloud.Exceptions;

namespace Steamfinity.Cloud.Services;

public sealed class LibraryManager : ILibraryManager
{
    private readonly ApplicationDbContext _context;
    private readonly IMembershipManager _membershipManager;
    private readonly ILimitProvider _limitProvider;

    public LibraryManager(ApplicationDbContext context, IMembershipManager membershipManager, ILimitProvider limitProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
        _limitProvider = limitProvider ?? throw new ArgumentNullException(nameof(limitProvider));
    }

    public IQueryable<Library> Libraries => _context.Libraries;

    public async Task<bool> ExistsAsync(Guid libraryId)
    {
        return await _context.Libraries.AnyAsync(l => l.Id == libraryId);
    }

    public async Task<Library?> FindByIdAsync(Guid libraryId)
    {
        return await _context.Libraries.FindAsync(libraryId);
    }

    public async Task<LibraryCreationResult> CreateAsync(Library library, Guid ownerId)
    {
        ArgumentNullException.ThrowIfNull(library, nameof(library));
        await ThrowIfUserNotExistsAsync(ownerId);

        var currentNumberOfLibraries = await _context.Memberships.CountAsync(m => m.UserId == ownerId);
        if (currentNumberOfLibraries >= _limitProvider.MaxLibrariesPerUser)
        {
            return LibraryCreationResult.LibraryLimitExceeded;
        }

        _ = await _context.Libraries.AddAsync(library);
        _ = await _context.SaveChangesAsync();

        var ownerAdditionResult = await _membershipManager.AddMemberAsync(library.Id, ownerId, MemberRole.Administrator);
        if (ownerAdditionResult != MemberAdditionResult.Success)
        {
            throw new InvalidOperationException($"The addition of the administrator resulted in: '{ownerAdditionResult}'.");
        }

        return LibraryCreationResult.Success;
    }

    public async Task UpdateAsync(Library library)
    {
        ArgumentNullException.ThrowIfNull(library, nameof(library));
        await ThrowIfLibraryNotExistsAsync(library.Id);

        _ = _context.Update(library);
        _ = await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Library library)
    {
        ArgumentNullException.ThrowIfNull(library, nameof(library));
        await ThrowIfLibraryNotExistsAsync(library.Id);

        _ = _context.Libraries.Remove(library);
        _ = await _context.SaveChangesAsync();
    }

    private async Task ThrowIfLibraryNotExistsAsync(Guid libraryId)
    {
        if (!await ExistsAsync(libraryId))
        {
            throw new NotFoundException($"The library '{libraryId}' was not found in the database.");
        }
    }

    private async Task ThrowIfUserNotExistsAsync(Guid userId)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == userId))
        {
            throw new NotFoundException($"The user '{userId}' was not found in the database.");
        }
    }
}
