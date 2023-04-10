using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Models;

public sealed record AccountQueryOptions
{
    public string? Search { get; init; }

    public SimpleColor? Color { get; init; }

    public SkillGroup? SkillGroup { get; init; }

    public OnlineAccountVisibility? OnlineVisibility { get; init; }

    public PrimeAccountVisibility? PrimeVisibility { get; init; }

    public BannedAccountVisibility? BannedVisibility { get; init; }

    public IEnumerable<string>? Hashtags { get; init; }
}
