namespace PetMach.Contracts.Tutors;

public sealed record UpsertTutorProfileRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string City,
    string State,
    string? Biography,
    bool ShowCity,
    bool AllowDiscovery);

public sealed record TutorProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Phone,
    string City,
    string State,
    string? Biography,
    bool ShowCity,
    bool AllowDiscovery,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
