using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud.Enums;

/// <summary>
/// Represents a role of a shared library member.
/// </summary>
/// <remarks>
/// Do not confuse with <see cref="ApplicationRole"/>.
/// </remarks>
public enum MemberRole
{
    Guest,
    Member,
    Administrator
}
