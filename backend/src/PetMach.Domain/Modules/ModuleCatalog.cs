namespace PetMach.Domain.Modules;

public static class ModuleCatalog
{
    public static IReadOnlyList<string> All { get; } = Array.AsReadOnly(
    [
        "Identity",
        "Tutors",
        "Dogs",
        "Health",
        "Discovery",
        "Matches",
        "Chat",
        "Meetings",
        "Partners",
        "Spaces",
        "Reservations",
        "Adoption",
        "Notifications",
        "Moderation",
        "Administration",
        "SharedKernel",
    ]);
}
