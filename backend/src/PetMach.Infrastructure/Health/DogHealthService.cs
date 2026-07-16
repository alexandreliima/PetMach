using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PetMach.Application.Health;
using PetMach.Contracts.Health;
using PetMach.Domain.Dogs;
using PetMach.Domain.Health;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Health;

internal sealed class DogHealthService(
    PetMachDbContext dbContext,
    IWebHostEnvironment environment,
    TimeProvider timeProvider) : IDogHealthService
{
    private const string Disclaimer = "Estas informações não substituem avaliação veterinária.";
    private const long MaximumProofLength = 5 * 1024 * 1024;
    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];

    public async Task<Result<DogHealthResponse>> GetAsync(Guid userId, Guid dogId, CancellationToken cancellationToken)
    {
        if (!await IsOwnedAsync(userId, dogId, cancellationToken))
            return Result.Failure<DogHealthResponse>(DogErrors.NotFound);

        VaccinationResponse[] vaccines = await dbContext.DogVaccinations
            .AsNoTracking()
            .Where(x => x.DogId == dogId)
            .OrderByDescending(x => x.AppliedOn)
            .Select(x => new VaccinationResponse(x.Id, x.VaccineName, x.AppliedOn, x.NextDoseOn, x.ProtectedProofKey != null))
            .ToArrayAsync(cancellationToken);
        DewormingResponse[] dewormings = await dbContext.DewormingRecords
            .AsNoTracking()
            .Where(x => x.DogId == dogId)
            .OrderByDescending(x => x.AppliedOn)
            .Select(x => new DewormingResponse(x.Id, x.ProductName, x.AppliedOn, x.NextDoseOn))
            .ToArrayAsync(cancellationToken);

        DateOnly today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        bool upToDate = vaccines.Length > 0 && vaccines.All(x => x.NextDoseOn is null || x.NextDoseOn >= today);
        return Result.Success(new DogHealthResponse(vaccines, dewormings, upToDate, Disclaimer));
    }

    public async Task<Result<VaccinationResponse>> AddVaccinationAsync(Guid userId, Guid dogId, CreateVaccinationRequest request, CancellationToken cancellationToken)
    {
        if (!await IsOwnedAsync(userId, dogId, cancellationToken))
            return Result.Failure<VaccinationResponse>(DogErrors.NotFound);

        try
        {
            DogVaccination item = new(dogId, request.VaccineName, request.AppliedOn, request.NextDoseOn, timeProvider.GetUtcNow());
            dbContext.DogVaccinations.Add(item);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(new VaccinationResponse(item.Id, item.VaccineName, item.AppliedOn, item.NextDoseOn, false));
        }
        catch (ArgumentException)
        {
            return Result.Failure<VaccinationResponse>(DogErrors.Invalid);
        }
    }

    public async Task<Result<DewormingResponse>> AddDewormingAsync(Guid userId, Guid dogId, CreateDewormingRequest request, CancellationToken cancellationToken)
    {
        if (!await IsOwnedAsync(userId, dogId, cancellationToken))
            return Result.Failure<DewormingResponse>(DogErrors.NotFound);

        try
        {
            DewormingRecord item = new(dogId, request.ProductName, request.AppliedOn, request.NextDoseOn, timeProvider.GetUtcNow());
            dbContext.DewormingRecords.Add(item);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(new DewormingResponse(item.Id, item.ProductName, item.AppliedOn, item.NextDoseOn));
        }
        catch (ArgumentException)
        {
            return Result.Failure<DewormingResponse>(DogErrors.Invalid);
        }
    }

    public async Task<Result<VaccinationProofResponse>> AddVaccinationProofAsync(
        Guid userId,
        Guid dogId,
        Guid vaccinationId,
        Stream content,
        long length,
        CancellationToken cancellationToken)
    {
        DogVaccination? vaccination = await OwnedVaccinationAsync(userId, dogId, vaccinationId, cancellationToken);
        if (vaccination is null) return Result.Failure<VaccinationProofResponse>(DogErrors.NotFound);
        if (length is <= 0 or > MaximumProofLength) return Result.Failure<VaccinationProofResponse>(HealthErrors.ProofInvalid);

        await using MemoryStream buffer = new();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != length || buffer.Length > MaximumProofLength)
            return Result.Failure<VaccinationProofResponse>(HealthErrors.ProofInvalid);

        (string? contentType, string? extension) = DetectProof(buffer.GetBuffer().AsSpan(0, (int)buffer.Length));
        if (contentType is null || extension is null)
            return Result.Failure<VaccinationProofResponse>(HealthErrors.ProofInvalid);

        string relativePath = Path.Combine("health", dogId.ToString("N"), $"{Guid.NewGuid():N}{extension}");
        string fullPath = StoragePath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, buffer.ToArray(), cancellationToken);

        string? previous = vaccination.ProtectedProofKey;
        vaccination.AttachProof(relativePath);
        await dbContext.SaveChangesAsync(cancellationToken);
        DeletePreviousProof(previous);
        return Result.Success(new VaccinationProofResponse(vaccination.Id, contentType, buffer.Length));
    }

    public async Task<Result<ProtectedHealthFile>> GetVaccinationProofAsync(Guid userId, Guid dogId, Guid vaccinationId, CancellationToken cancellationToken)
    {
        DogVaccination? vaccination = await OwnedVaccinationAsync(userId, dogId, vaccinationId, cancellationToken);
        if (vaccination?.ProtectedProofKey is null)
            return Result.Failure<ProtectedHealthFile>(HealthErrors.ProofNotFound);

        string path = StoragePath(vaccination.ProtectedProofKey);
        if (!File.Exists(path)) return Result.Failure<ProtectedHealthFile>(HealthErrors.ProofNotFound);
        byte[] content = await File.ReadAllBytesAsync(path, cancellationToken);
        string extension = Path.GetExtension(path).ToLowerInvariant();
        string contentType = extension switch { ".pdf" => "application/pdf", ".jpg" => "image/jpeg", ".png" => "image/png", _ => "application/octet-stream" };
        return Result.Success(new ProtectedHealthFile(content, contentType, $"comprovante{extension}"));
    }

    private Task<bool> IsOwnedAsync(Guid userId, Guid dogId, CancellationToken cancellationToken) =>
        dbContext.Dogs.AnyAsync(x => x.Id == dogId && x.OwnerUserId == userId && x.Status != DogProfileStatus.Removed, cancellationToken);

    private Task<DogVaccination?> OwnedVaccinationAsync(Guid userId, Guid dogId, Guid vaccinationId, CancellationToken cancellationToken) =>
        dbContext.DogVaccinations.SingleOrDefaultAsync(
            x => x.Id == vaccinationId && x.DogId == dogId && dbContext.Dogs.Any(d => d.Id == dogId && d.OwnerUserId == userId && d.Status != DogProfileStatus.Removed),
            cancellationToken);

    private string StoragePath(string relativePath) => Path.Combine(environment.ContentRootPath, ".dev-storage", relativePath);

    private void DeletePreviousProof(string? relativePath)
    {
        if (relativePath is null) return;
        string path = StoragePath(relativePath);
        if (File.Exists(path)) File.Delete(path);
    }

    private static (string? ContentType, string? Extension) DetectProof(ReadOnlySpan<byte> content)
    {
        if (content.Length >= 5 && content[..5].SequenceEqual("%PDF-"u8)) return ("application/pdf", ".pdf");
        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF) return ("image/jpeg", ".jpg");
        if (content.Length >= 8 && content[..8].SequenceEqual(PngSignature)) return ("image/png", ".png");
        return (null, null);
    }
}
