namespace Steamfinity.Cloud.Services;

public sealed class LimitProvider : ILimitProvider
{
    private readonly IConfiguration _configuration;

    public LimitProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public int MaxLibrariesPerUser => _configuration.GetValue("Limits:MaxLibrariesPerUser", int.MaxValue);

    public int MaxMembersPerLibrary => _configuration.GetValue("Limits:MaxMembersPerLibrary", int.MaxValue);

    public int MaxAccountsPerLibrary => _configuration.GetValue("Limits:MaxAccountsPerLibrary", int.MaxValue);

    public int MaxHashtagsPerAccount => _configuration.GetValue("Limits:MaxHashtagsPerAccount", int.MaxValue);
}
