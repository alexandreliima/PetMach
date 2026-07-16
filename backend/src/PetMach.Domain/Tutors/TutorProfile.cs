using PetMach.Domain.SharedKernel;

namespace PetMach.Domain.Tutors;

public sealed class TutorProfile
{
    private TutorProfile() { }

    private TutorProfile(Guid userId, string firstName, string lastName, string? phone, string city, string state, string? biography, bool showCity, bool allowDiscovery, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAtUtc = now;
        Apply(firstName, lastName, phone, city, state, biography, showCity, allowDiscovery, now);
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string? Biography { get; private set; }
    public bool ShowCity { get; private set; }
    public bool AllowDiscovery { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static Result<TutorProfile> Create(Guid userId, string firstName, string lastName, string? phone, string city, string state, string? biography, bool showCity, bool allowDiscovery, DateTimeOffset now)
    {
        if (!IsValid(userId, firstName, lastName, city, state, biography))
            return Result.Failure<TutorProfile>(TutorErrors.InvalidProfile);
        return Result.Success(new TutorProfile(userId, firstName, lastName, phone, city, state, biography, showCity, allowDiscovery, now));
    }

    public Result Update(string firstName, string lastName, string? phone, string city, string state, string? biography, bool showCity, bool allowDiscovery, DateTimeOffset now)
    {
        if (!IsValid(UserId, firstName, lastName, city, state, biography)) return Result.Failure(TutorErrors.InvalidProfile);
        Apply(firstName, lastName, phone, city, state, biography, showCity, allowDiscovery, now);
        return Result.Success();
    }

    private void Apply(string firstName, string lastName, string? phone, string city, string state, string? biography, bool showCity, bool allowDiscovery, DateTimeOffset now)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        Biography = string.IsNullOrWhiteSpace(biography) ? null : biography.Trim();
        ShowCity = showCity;
        AllowDiscovery = allowDiscovery;
        UpdatedAtUtc = now;
    }

    private static bool IsValid(Guid userId, string firstName, string lastName, string city, string state, string? biography) =>
        userId != Guid.Empty &&
        !string.IsNullOrWhiteSpace(firstName) && firstName.Trim().Length <= 100 &&
        !string.IsNullOrWhiteSpace(lastName) && lastName.Trim().Length <= 100 &&
        !string.IsNullOrWhiteSpace(city) && city.Trim().Length <= 120 &&
        !string.IsNullOrWhiteSpace(state) && state.Trim().Length is >= 2 and <= 50 &&
        (biography is null || biography.Length <= 1000);
}
