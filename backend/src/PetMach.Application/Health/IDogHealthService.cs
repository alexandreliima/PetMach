using PetMach.Contracts.Health;
using PetMach.Domain.SharedKernel;

namespace PetMach.Application.Health;

public interface IDogHealthService
{
    Task<Result<DogHealthResponse>> GetAsync(Guid userId, Guid dogId, CancellationToken cancellationToken);
    Task<Result<VaccinationResponse>> AddVaccinationAsync(Guid userId, Guid dogId, CreateVaccinationRequest request, CancellationToken cancellationToken);
    Task<Result<DewormingResponse>> AddDewormingAsync(Guid userId, Guid dogId, CreateDewormingRequest request, CancellationToken cancellationToken);
    Task<Result<VaccinationProofResponse>> AddVaccinationProofAsync(Guid userId, Guid dogId, Guid vaccinationId, Stream content, long length, CancellationToken cancellationToken);
    Task<Result<ProtectedHealthFile>> GetVaccinationProofAsync(Guid userId, Guid dogId, Guid vaccinationId, CancellationToken cancellationToken);
}

public sealed record ProtectedHealthFile(byte[] Content, string ContentType, string FileName);
