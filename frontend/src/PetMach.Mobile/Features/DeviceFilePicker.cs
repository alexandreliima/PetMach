using PetMach.Mobile.Core.Features;

namespace PetMach.Mobile.Features;

public sealed class DeviceFilePicker : IDeviceFilePicker
{
    private const int MaximumLength = 5 * 1024 * 1024;

    public async Task<PickedFile?> PickPhotoAsync(CancellationToken cancellationToken)
    {
        IEnumerable<FileResult> results = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "Escolha uma foto do cão", SelectionLimit = 1 });
        FileResult? result = results.FirstOrDefault();
        return result is null ? null : await ReadAsync(result, false, cancellationToken);
    }

    public async Task<PickedFile?> PickHealthProofAsync(CancellationToken cancellationToken)
    {
        FileResult? result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Escolha o comprovante da vacina" });
        return result is null ? null : await ReadAsync(result, true, cancellationToken);
    }

    public async Task<PickedFile?> PickReportEvidenceAsync(CancellationToken cancellationToken)
    {
        FileResult? result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Escolha uma evidência" });
        return result is null ? null : await ReadAsync(result, true, cancellationToken);
    }

    private static async Task<PickedFile> ReadAsync(FileResult result, bool allowPdf, CancellationToken cancellationToken)
    {
        string extension = Path.GetExtension(result.FileName).ToLowerInvariant();
        string contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" when !allowPdf => "image/webp",
            ".pdf" when allowPdf => "application/pdf",
            _ => throw new InvalidOperationException("Tipo de arquivo não permitido."),
        };

        await using Stream input = await result.OpenReadAsync();
        await using MemoryStream output = new();
        await input.CopyToAsync(output, cancellationToken);
        if (output.Length is <= 0 or > MaximumLength) throw new InvalidOperationException("O arquivo deve ter no máximo 5 MB.");
        return new PickedFile(result.FileName, contentType, output.ToArray());
    }
}
