namespace Steamfinity.Cloud.Services;

public interface IPermissionManager
{
    Task<bool> CanViewLibraryAsync(Guid libraryId, Guid userId);

    Task<bool> CanManageLibrary(Guid libraryId, Guid userId);

    Task<bool> CanManageMembersAsync(Guid libraryId, Guid userId);

    Task<bool> CanManageAccountsAsync(Guid libraryId, Guid userId);

    Task<bool> CanViewPasswordsAsync(Guid libraryId, Guid userId);
}
