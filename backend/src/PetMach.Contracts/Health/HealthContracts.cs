namespace PetMach.Contracts.Health;

public sealed record CreateVaccinationRequest(string VaccineName, DateOnly AppliedOn, DateOnly? NextDoseOn);
public sealed record VaccinationResponse(Guid Id, string VaccineName, DateOnly AppliedOn, DateOnly? NextDoseOn, bool HasProof);
public sealed record VaccinationProofResponse(Guid VaccinationId, string ContentType, long Length);
public sealed record CreateDewormingRequest(string ProductName, DateOnly AppliedOn, DateOnly? NextDoseOn);
public sealed record DewormingResponse(Guid Id, string ProductName, DateOnly AppliedOn, DateOnly? NextDoseOn);
public sealed record DogHealthResponse(IReadOnlyCollection<VaccinationResponse> Vaccinations, IReadOnlyCollection<DewormingResponse> Dewormings, bool VaccinationUpToDate, string Disclaimer);
