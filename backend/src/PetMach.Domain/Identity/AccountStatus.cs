namespace PetMach.Domain.Identity;

public enum AccountStatus
{
    PendingConfirmation = 0,
    Active = 1,
    Suspended = 2,
    Anonymized = 3,
}
