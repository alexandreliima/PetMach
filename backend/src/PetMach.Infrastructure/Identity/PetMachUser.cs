using Microsoft.AspNetCore.Identity;
using PetMach.Domain.Identity;

namespace PetMach.Infrastructure.Identity;

public sealed class PetMachUser : IdentityUser<Guid>
{
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateOnly BirthDate { get; set; }
    public AccountStatus Status { get; set; } = AccountStatus.PendingConfirmation;
    public DateTimeOffset? SuspendedAtUtc { get; set; }
    public DateTimeOffset? AnonymizedAtUtc { get; set; }
}
