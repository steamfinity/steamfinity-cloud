using System.Configuration;

namespace Steamfinity.Cloud.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a required app setting is missing.
/// </summary>
public sealed class ConfigurationMissingException : ConfigurationErrorsException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationMissingException"/> class with a specified setting name.
    /// </summary>
    /// <param name="settingName">The name of the missing setting.</param>
    public ConfigurationMissingException(string settingName) : base($"The required property '{settingName}' was not found in the app settings.") { }
}
