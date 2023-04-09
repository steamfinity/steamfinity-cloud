using Microsoft.EntityFrameworkCore;
using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public sealed class LibraryManager : ILibraryManager
{
    private readonly ApplicationDbContext _context;
    private readonly ILimitProvider _limitProvider;

    public LibraryManager(
        ApplicationDbContext context,
        ILimitProvider limitProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
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

        // Ensure the user has not reached the library limit:
        var currentNumberOfLibraries = await _context.Memberships.CountAsync(m => m.UserId == ownerId);
        if (currentNumberOfLibraries >= _limitProvider.MaxLibrariesPerUser)
        {
            return LibraryCreationResult.LibraryLimitExceeded;
        }

        _ = await _context.Libraries.AddAsync(library);
        _ = await _context.SaveChangesAsync();

        // Create a membership for the owner of the library:
        var ownerMembership = new Membership
        {
            LibraryId = library.Id,
            UserId = ownerId,
            Role = MemberRole.Administrator
        };

        _ = await _context.Memberships.AddAsync(ownerMembership);
        _ = await _context.SaveChangesAsync();

        return LibraryCreationResult.Success;
    }

    public async Task UpdateAsync(Library library)
    {
        ArgumentNullException.ThrowIfNull(library, nameof(library));

        _ = _context.Update(library);
        _ = await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Library library)
    {
        ArgumentNullException.ThrowIfNull(nameof(library));

        _ = _context.Libraries.Attach(library);
        _ = _context.Libraries.Remove(library);

        _ = await _context.SaveChangesAsync();
    }
}
