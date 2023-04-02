namespace Steamfinity.Cloud.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an identity error occurs in the Steamfinity Cloud.
/// </summary>
public sealed class IdentityException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityException"/> class with a specified error code.
    /// </summary>
    /// <param name="errorCode">The code that represents the identity error.</param>
    public IdentityException(string errorCode) : base($"An identity error has occurred: '{errorCode}'.") { }
}
