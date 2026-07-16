using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PetMach.Application.Dogs;
using PetMach.Contracts.Dogs;
using PetMach.Domain.Dogs;
using PetMach.Domain.SharedKernel;
using PetMach.Infrastructure.Persistence;

namespace PetMach.Infrastructure.Dogs;

internal sealed class DogPhotoService(PetMachDbContext dbContext, IWebHostEnvironment environment, TimeProvider timeProvider) : IDogPhotoService
{
    private const long MaximumLength = 5 * 1024 * 1024;
    public async Task<Result<DogPhotoResponse>> AddAsync(Guid userId, Guid dogId, Stream content, string contentType, long length, CancellationToken cancellationToken)
    {
        bool owned = await dbContext.Dogs.AnyAsync(x => x.Id == dogId && x.OwnerUserId == userId && x.Status != DogProfileStatus.Removed, cancellationToken);
        if (!owned) return Result.Failure<DogPhotoResponse>(DogErrors.NotFound);
        if (length is <= 0 or > MaximumLength) return Result.Failure<DogPhotoResponse>(DogErrors.PhotoInvalid);
        await using MemoryStream buffer = new();
        await content.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != length || buffer.Length > MaximumLength) return Result.Failure<DogPhotoResponse>(DogErrors.PhotoInvalid);
        byte[] file = buffer.ToArray();
        (string? actualType, string? extension) = Detect(file.AsSpan(0, Math.Min(file.Length, 12)));
        if (actualType is null) return Result.Failure<DogPhotoResponse>(DogErrors.PhotoInvalid);
        string relative = Path.Combine("dogs", dogId.ToString("N"), $"{Guid.NewGuid():N}{extension}");
        string full = Path.Combine(environment.ContentRootPath, ".dev-storage", relative); Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await File.WriteAllBytesAsync(full, file, cancellationToken);
        bool primary = !await dbContext.DogPhotos.AnyAsync(x => x.DogId == dogId, cancellationToken);
        DogPhoto photo = new(dogId, relative, actualType, length, primary, timeProvider.GetUtcNow()); dbContext.DogPhotos.Add(photo); await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(new DogPhotoResponse(photo.Id, dogId, actualType, length, primary));
    }
    public async Task<IReadOnlyCollection<DogPhotoResponse>> ListAsync(Guid userId, Guid dogId, CancellationToken cancellationToken)
    {
        bool owned = await dbContext.Dogs.AnyAsync(x => x.Id == dogId && x.OwnerUserId == userId && x.Status != DogProfileStatus.Removed, cancellationToken);
        if (!owned) return [];
        return await dbContext.DogPhotos.AsNoTracking().Where(x => x.DogId == dogId).OrderByDescending(x => x.IsPrimary).Select(x => new DogPhotoResponse(x.Id, x.DogId, x.ContentType, x.Length, x.IsPrimary)).ToArrayAsync(cancellationToken);
    }
    private static (string?, string?) Detect(ReadOnlySpan<byte> h)
    {
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ("image/jpeg", ".jpg");
        if (h.Length >= 8 && h[..8].SequenceEqual(PngSignature)) return ("image/png", ".png");
        if (h.Length >= 12 && h[..4].SequenceEqual("RIFF"u8) && h[8..12].SequenceEqual("WEBP"u8)) return ("image/webp", ".webp");
        return (null, null);
    }

    private static readonly byte[] PngSignature = [137, 80, 78, 71, 13, 10, 26, 10];
}
