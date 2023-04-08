namespace Steamfinity.Cloud.Services;

/// <summary>
/// <inheritdoc cref="ILimitProvider"/>
/// </summary>
public sealed class LimitProvider : ILimitProvider
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="LimitProvider"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="configuration"/> is <see langword="null"/>.</exception>
    public LimitProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public int MaxLibrariesPerUser => _configuration.GetValue("Limits:MaxLibrariesPerUser", 100);

    public int MaxMembersPerLibrary => _configuration.GetValue("Limits:MaxMembersPerLibrary", 100);

    public int MaxAccountsPerLibrary => _configuration.GetValue("Limits:MaxAccountsPerLibrary", 100);
}
