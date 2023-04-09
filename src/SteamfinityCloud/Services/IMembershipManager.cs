﻿using Steamfinity.Cloud.Entities;
using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Services;

public interface IMembershipManager
{
    IQueryable<Membership> Memberships { get; }

    Task<Membership?> FindByIdAsync(Guid libraryId, Guid userId);

    Task<MemberAdditionResult> AddMemberAsync(Guid libraryId, Guid userId, MemberRole role);

    Task<MemberRoleChangeResult> ChangeMemberRoleAsync(Guid libraryId, Guid userId, MemberRole role);

    Task<MemberRemovalResult> RemoveMemberAsync(Guid libraryId, Guid userId);
}
