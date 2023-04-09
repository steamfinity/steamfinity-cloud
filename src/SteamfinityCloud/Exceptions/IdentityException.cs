namespace Steamfinity.Cloud.Exceptions;

public sealed class IdentityException : Exception
{
    public IdentityException(string errorCode) : base($"An identity error has occurred: '{errorCode}'.") { }
}
