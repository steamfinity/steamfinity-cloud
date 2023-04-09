namespace Steamfinity.Cloud.Services;

public sealed class LimitProvider : ILimitProvider
{
    private readonly IConfiguration _configuration;

    public LimitProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public int MaxLibrariesPerUser => _configuration.GetValue("Limits:MaxLibrariesPerUser", 100);

    public int MaxMembersPerLibrary => _configuration.GetValue("Limits:MaxMembersPerLibrary", 100);

    public int MaxAccountsPerLibrary => _configuration.GetValue("Limits:MaxAccountsPerLibrary", 100);

    public int MaxHashtagsPerAccount => _configuration.GetValue("Limits:MaxHashtagsPerAccount", 25);
}
