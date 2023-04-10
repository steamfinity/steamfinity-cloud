using System.Diagnostics.CodeAnalysis;

namespace Steamfinity.Cloud.Models;

public sealed record ApiError
{
    [SetsRequiredMembers]
    public ApiError(string code, string? message = null)
    {
        Code = code;
        Message = message;
    }

    public required string Code { get; init; }

    public string? Message { get; init; }
}
