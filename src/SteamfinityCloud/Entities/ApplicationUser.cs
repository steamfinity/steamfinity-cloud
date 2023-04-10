using Microsoft.AspNetCore.Identity;

namespace Steamfinity.Cloud.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public bool IsSuspended { get; set; }

    public DateTimeOffset SignUpTime { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastSignInTime { get; set; }

    public ICollection<Membership> Memberships { get; } = null!;

    public ICollection<AccountInteraction> AccountInteractions { get; } = null!;
}
