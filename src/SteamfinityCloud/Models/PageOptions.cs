using Steamfinity.Cloud.Constants;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Steamfinity.Cloud.Models;

public sealed record PageOptions
{
    [Range(1, int.MaxValue)]
    [DefaultValue(1)]
    public int PageNumber { get; init; } = 1;

    [Range(1, OtherConstants.MaximumPageSize)]
    [DefaultValue(OtherConstants.DefaultPageSize)]
    public int PageSize { get; init; } = OtherConstants.DefaultPageSize;
}
