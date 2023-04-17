using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public sealed class PermissionManager : IPermissionManager
{
    private readonly IMembershipManager _membershipManager;

    public PermissionManager(IMembershipManager membershipManager)
    {
        _membershipManager = membershipManager ?? throw new ArgumentNullException(nameof(membershipManager));
    }

    public async Task<bool> CanViewLibraryAsync(Guid libraryId, Guid userId)
    {
        return await GetMemberRoleAsync(libraryId, userId) >= MemberRole.Guest;
    }

    public async Task<bool> CanManageLibrary(Guid libraryId, Guid userId)
    {
        return await GetMemberRoleAsync(libraryId, userId) == MemberRole.Administrator;
    }

    public async Task<bool> CanManageMembersAsync(Guid libraryId, Guid userId)
    {
        return await GetMemberRoleAsync(libraryId, userId) == MemberRole.Administrator;
    }

    public async Task<bool> CanManageAccountsAsync(Guid libraryId, Guid userId)
    {
        return await GetMemberRoleAsync(libraryId, userId) >= MemberRole.Member;
    }

    public async Task<bool> CanViewPasswordsAsync(Guid libraryId, Guid userId)
    {
        return await GetMemberRoleAsync(libraryId, userId) >= MemberRole.Member;
    }

    private async Task<MemberRole?> GetMemberRoleAsync(Guid libraryId, Guid userId)
    {
        return (await _membershipManager.FindByIdAsync(libraryId, userId))?.Role;
    }
}
