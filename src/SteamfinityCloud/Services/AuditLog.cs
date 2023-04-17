using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public sealed class AuditLog : IAuditLog
{
    private readonly ApplicationDbContext _context;

    public AuditLog(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IQueryable<Activity> Activities => _context.Activities;

    public async Task LogActivityAsync(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(activity, nameof(activity));

        _ = await _context.Activities.AddAsync(activity);
        _ = await _context.SaveChangesAsync();
    }

    public async Task LogTokenRefreshAsync(Guid userId)
    {
        var activity = new Activity
        {
            Type = ActivityType.TokenRefresh,
            InstigatorId = userId,
            TargetUserId = userId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserSignUpAsync(Guid userId)
    {
        var activity = new Activity
        {
            Type = ActivityType.UserSignUp,
            InstigatorId = userId,
            TargetUserId = userId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserSignInAsync(Guid userId)
    {
        var activity = new Activity
        {
            Type = ActivityType.UserSignIn,
            InstigatorId = userId,
            TargetUserId = userId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserNameChangeAsync(Guid instigatorId, Guid targetUserId, string? previousName, string? newName)
    {
        var activity = new Activity
        {
            Type = ActivityType.UserNameChange,
            InstigatorId = instigatorId,
            TargetUserId = targetUserId,
            PreviousValue = previousName,
            NewValue = newName
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserPasswordChangeAsync(Guid instigatorId, Guid targetUserId)
    {
        var activity = new Activity
        {
            Type = ActivityType.UserPasswordChange,
            InstigatorId = instigatorId,
            TargetUserId = targetUserId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserRoleAddition(Guid instigatorId, Guid targetUserId, string roleName)
    {
        ArgumentException.ThrowIfNullOrEmpty(roleName, nameof(roleName));

        var activity = new Activity
        {
            Type = ActivityType.UserRoleAddition,
            InstigatorId = instigatorId,
            TargetUserId = targetUserId,
            NewValue = roleName
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserRoleRemoval(Guid instigatorId, Guid targetUserId, string roleName)
    {
        ArgumentException.ThrowIfNullOrEmpty(roleName, nameof(roleName));

        var activity = new Activity
        {
            Type = ActivityType.UserRoleRemoval,
            InstigatorId = instigatorId,
            TargetUserId = targetUserId,
            PreviousValue = roleName
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserSuspensionChangeAsync(Guid instigatorId, Guid targetUserId, bool wasSuspended, bool isNowSuspended)
    {
        var activity = new Activity
        {
            Type = ActivityType.UserSuspensionChange,
            InstigatorId = instigatorId,
            TargetUserId = targetUserId,
            PreviousValue = wasSuspended.ToString(),
            NewValue = isNowSuspended.ToString()
        };

        await LogActivityAsync(activity);
    }

    public async Task LogUserDeletionAsync(Guid instigatorId, Guid targetUserId)
    {
        var activity = new Activity
        {
            Type = ActivityType.UserDeletion,
            InstigatorId = instigatorId,
            TargetUserId = targetUserId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogLibraryCreationAsync(Guid instigatorId, Guid libraryId)
    {
        var activity = new Activity
        {
            Type = ActivityType.LibraryCreation,
            InstigatorId = instigatorId,
            TargetLibraryId = libraryId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogLibraryNameChangeAsync(Guid instigatorId, Guid libraryId, string previousName, string newName)
    {
        ArgumentException.ThrowIfNullOrEmpty(previousName, nameof(previousName));
        ArgumentException.ThrowIfNullOrEmpty(newName, nameof(newName));

        var activity = new Activity
        {
            Type = ActivityType.LibraryNameChange,
            InstigatorId = instigatorId,
            TargetLibraryId = libraryId,
            PreviousValue = previousName,
            NewValue = newName
        };

        await LogActivityAsync(activity);
    }

    public async Task LogLibraryDescriptionChangeAsync(Guid instigatorId, Guid libraryId)
    {
        var activity = new Activity
        {
            Type = ActivityType.LibraryDescriptionChange,
            InstigatorId = instigatorId,
            TargetLibraryId = libraryId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogLibraryDeletionAsync(Guid instigatorId, Guid libraryId)
    {
        var activity = new Activity
        {
            Type = ActivityType.LibraryDeletion,
            InstigatorId = instigatorId,
            TargetLibraryId = libraryId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogMemberAdditionAsync(Guid instigatorId, Guid libraryId, Guid memberId)
    {
        var activity = new Activity
        {
            Type = ActivityType.MemberAddition,
            InstigatorId = instigatorId,
            TargetUserId = memberId,
            TargetLibraryId = libraryId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogMemberRoleChangeAsync(Guid instigatorId, Guid libraryId, Guid memberId, MemberRole previousRole, MemberRole newRole)
    {
        var activity = new Activity
        {
            Type = ActivityType.MemberRoleChange,
            InstigatorId = instigatorId,
            TargetUserId = memberId,
            TargetLibraryId = libraryId,
            PreviousValue = previousRole.ToString(),
            NewValue = newRole.ToString()
        };

        await LogActivityAsync(activity);
    }

    public async Task LogMemberRemovalAsync(Guid instigatorId, Guid libraryId, Guid memberId)
    {
        var activity = new Activity
        {
            Type = ActivityType.MemberRemoval,
            InstigatorId = instigatorId,
            TargetUserId = memberId,
            TargetLibraryId = libraryId,
        };

        await LogActivityAsync(activity);
    }

    public async Task LogAccountAdditionAsync(Guid instigatorId, Guid libraryId, Guid accountId)
    {
        var activity = new Activity
        {
            Type = ActivityType.AccountAddition,
            InstigatorId = instigatorId,
            TargetLibraryId = libraryId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogAccountSignInAsync(Guid instigatorId, Guid accountId)
    {
        var activity = new Activity
        {
            Type = ActivityType.AccountSignIn,
            InstigatorId = instigatorId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogAccountNameChangeAsync(Guid instigatorId, Guid accountId, string? previousName, string? newName)
    {
        var activity = new Activity
        {
            Type = ActivityType.AccountNameChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId,
            PreviousValue = previousName,
            NewValue = newName
        };

        await LogActivityAsync(activity);
    }

    public async Task LogAccountPasswordChangeAsync(Guid instigatorId, Guid accountId)
    {
        var activity = new Activity
        {
            Type = ActivityType.AccountPasswordChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogAliasChangeAsync(Guid instigatorId, Guid accountId, string? previousAlias, string? newAlias)
    {
        var activity = new Activity
        {
            Type = ActivityType.AliasChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId,
            PreviousValue = previousAlias,
            NewValue = newAlias
        };

        await LogActivityAsync(activity);
    }

    public async Task LogPrimeStatusChangeAsync(Guid instigatorId, Guid accountId, bool hadPrimeStatus, bool hasNowPrimeStatus)
    {
        var activity = new Activity
        {
            Type = ActivityType.PrimeStatusChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId,
            PreviousValue = hadPrimeStatus.ToString(),
            NewValue = hasNowPrimeStatus.ToString()
        };

        await LogActivityAsync(activity);
    }

    public async Task LogSkillGroupChangeAsync(Guid instigatorId, Guid accountId, SkillGroup previousSkillGroup, SkillGroup newSkillGroup)
    {
        var activity = new Activity
        {
            Type = ActivityType.SkillGroupChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId,
            PreviousValue = previousSkillGroup.ToString(),
            NewValue = newSkillGroup.ToString()
        };

        await LogActivityAsync(activity);
    }

    public async Task LogCooldownExpirationTimeChangeAsync(Guid instigatorId, Guid accountId, DateTimeOffset? previousTime, DateTimeOffset? newTime)
    {
        var activity = new Activity
        {
            Type = ActivityType.CooldownExpirationTimeChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId,
            PreviousValue = previousTime.ToString(),
            NewValue = newTime.ToString()
        };

        await LogActivityAsync(activity);
    }

    public async Task LogLaunchParametersChangeAsync(Guid instigatorId, Guid accountId)
    {
        var activity = new Activity
        {
            Type = ActivityType.LaunchParametersChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogNotesChangeAsync(Guid instigatorId, Guid accountId)
    {
        var activity = new Activity
        {
            Type = ActivityType.NotesChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogHashtagsChangeAsync(Guid instigatorId, Guid accountId)
    {
        var activity = new Activity
        {
            Type = ActivityType.HashtagsChange,
            InstigatorId = instigatorId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(activity);
    }

    public async Task LogAccountTransferAsync(Guid instigatorId, Guid accountId, Guid previousLibraryId, Guid newLibraryId)
    {
        var outgoingActivity = new Activity
        {
            Type = ActivityType.OutgoingAccountTransfer,
            InstigatorId = instigatorId,
            TargetLibraryId = previousLibraryId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(outgoingActivity);

        var incomingActivity = new Activity
        {
            Type = ActivityType.IncomingAccountTransfer,
            InstigatorId = instigatorId,
            TargetLibraryId = newLibraryId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(incomingActivity);
    }

    public async Task LogAccountRemoval(Guid instigatorId, Guid libraryId, Guid accountId)
    {
        var activity = new Activity
        {
            Type = ActivityType.AccountRemoval,
            InstigatorId = instigatorId,
            TargetLibraryId = libraryId,
            TargetAccountId = accountId
        };

        await LogActivityAsync(activity);
    }
}
