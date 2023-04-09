using System.Configuration;

namespace Steamfinity.Cloud.Exceptions;

public sealed class ConfigurationMissingException : ConfigurationErrorsException
{
    public ConfigurationMissingException(string settingName) : base($"The required property '{settingName}' was not found in the app settings.") { }
}
