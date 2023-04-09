namespace Steamfinity.Cloud.Models;

public sealed record LibraryOverview
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}
