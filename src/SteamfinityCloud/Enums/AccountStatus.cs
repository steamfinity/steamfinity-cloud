using Steamfinity.Cloud.Entities;

namespace Steamfinity.Cloud.Enums;

/// <summary>
/// Represents a status of a <see cref="SteamAccount"/>.
/// </summary>
public enum AccountStatus
{
    Offline,
    Online,
    Busy,
    Away,
    Snooze,
    LookingToTrade,
    LookingToPlay
}
