using Steamfinity.Cloud.Enums;

namespace Steamfinity.Cloud.Models;

public sealed record AccountQueryOptions
{
    public string? Search { get; init; }

    public string? Hashtag { get; init; }

    public SimpleColor? Color { get; init; }

    public SkillGroup? SkillGroup { get; init; }

    public OnlineAccountVisibility? OnlineVisibility { get; init; }

    public PrimeAccountVisibility? PrimeVisibility { get; init; }

    public BannedAccountVisibility? BannedVisibility { get; init; }

    public AccountSortingCriteria AccountSortingCriteria { get; init; }

    public AccountSortingOrder AccountSortingOrder { get; init; }
}
