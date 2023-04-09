namespace Steamfinity.Cloud.Services;

public interface ILimitProvider
{
    int MaxLibrariesPerUser { get; }

    int MaxMembersPerLibrary { get; }

    int MaxAccountsPerLibrary { get; }
}
