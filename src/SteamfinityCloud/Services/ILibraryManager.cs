using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public interface ILibraryManager
{
    IQueryable<Library> Libraries { get; }

    Task<bool> ExistsAsync(Guid libraryId);

    Task<Library?> FindByIdAsync(Guid libraryId);

    Task<LibraryCreationResult> CreateAsync(Library library, Guid ownerId);

    Task UpdateAsync(Library library);

    Task DeleteAsync(Library library);
}
