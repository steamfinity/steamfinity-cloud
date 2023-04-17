using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public interface IAuditLog
{
    IQueryable<Activity> Activities { get; }

    Task LogActivityAsync(Activity activity);

    Task LogTokenRefreshAsync(Guid userId);

    Task LogUserSignUpAsync(Guid userId);

    Task LogUserSignInAsync(Guid userId);

    Task LogUserNameChangeAsync(Guid instigatorId, Guid targetUserId, string? previousName, string? newName);

    Task LogUserPasswordChangeAsync(Guid instigatorId, Guid targetUserId);

    Task LogUserRoleAddition(Guid instigatorId, Guid targetUserId, string roleName);

    Task LogUserRoleRemoval(Guid instigatorId, Guid targetUserId, string roleName);

    Task LogUserSuspensionChangeAsync(Guid instigatorId, Guid targetUserId, bool wasSuspended, bool isNowSuspended);

    Task LogUserDeletionAsync(Guid instigatorId, Guid targetUserId);

    Task LogLibraryCreationAsync(Guid instigatorId, Guid libraryId);

    Task LogLibraryNameChangeAsync(Guid instigatorId, Guid libraryId, string previousName, string newName);

    Task LogLibraryDescriptionChangeAsync(Guid instigatorId, Guid libraryId);

    Task LogLibraryDeletionAsync(Guid instigatorId, Guid libraryId);

    Task LogMemberAdditionAsync(Guid instigatorId, Guid libraryId, Guid memberId);

    Task LogMemberRoleChangeAsync(Guid instigatorId, Guid libraryId, Guid memberId, MemberRole previousRole, MemberRole newRole);

    Task LogMemberRemovalAsync(Guid instigatorId, Guid libraryId, Guid memberId);

    Task LogAccountAdditionAsync(Guid instigatorId, Guid libraryId, Guid accountId);

    Task LogAccountSignInAsync(Guid instigatorId, Guid accountId);

    Task LogAccountNameChangeAsync(Guid instigatorId, Guid accountId, string? previousName, string? newName);

    Task LogAccountPasswordChangeAsync(Guid instigatorId, Guid accountId);

    Task LogAliasChangeAsync(Guid instigatorId, Guid accountId, string? previousAlias, string? newAlias);

    Task LogPrimeStatusChangeAsync(Guid instigatorId, Guid accountId, bool hadPrimeStatus, bool hasNowPrimeStatus);

    Task LogSkillGroupChangeAsync(Guid instigatorId, Guid accountId, SkillGroup previousSkillGroup, SkillGroup newSkillGroup);

    Task LogCooldownExpirationTimeChangeAsync(Guid instigatorId, Guid accountId, DateTimeOffset? previousTime, DateTimeOffset? newTime);

    Task LogLaunchParametersChangeAsync(Guid instigatorId, Guid accountId);

    Task LogNotesChangeAsync(Guid instigatorId, Guid accountId);

    Task LogHashtagsChangeAsync(Guid instigatorId, Guid accountId);

    Task LogAccountTransferAsync(Guid instigatorId, Guid accountId, Guid previousLibraryId, Guid newLibraryId);

    Task LogAccountRemoval(Guid instigatorId, Guid libraryId, Guid accountId);
}
