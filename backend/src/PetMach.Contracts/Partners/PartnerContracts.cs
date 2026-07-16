namespace PetMach.Contracts.Partners;

public sealed record CreatePartnerRequest(string LegalName, string DisplayName, string RegistrationNumber, string City, string State, string TimeZoneId);
public sealed record PartnerManagementResponse(Guid Id, string LegalName, string DisplayName, string RegistrationNumber, string City, string State, string TimeZoneId, bool IsActive);
public sealed record CreateSpaceRequest(string Name, string Description, int Capacity, decimal InformationalPrice);
public sealed record PartnerSpaceResponse(Guid Id, Guid EstablishmentId, string EstablishmentName, string Name, string Description, int Capacity, decimal InformationalPrice, string City, string State, string TimeZoneId);
public sealed record CreateAvailabilityRequest(DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc);
public sealed record SpaceAvailabilityResponse(Guid Id, Guid SpaceId, DateTimeOffset StartsAtUtc, DateTimeOffset EndsAtUtc, bool IsAvailable);
