using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace PetMach.Infrastructure.Identity;

internal sealed class DevelopmentEmailSender(IWebHostEnvironment environment) : ITransactionalEmailSender
{
    public Task SendEmailConfirmationAsync(string email, Guid userId, string token, CancellationToken cancellationToken) =>
        WriteAsync("confirmacao", email, userId, token, cancellationToken);

    public Task SendPasswordResetAsync(string email, Guid userId, string token, CancellationToken cancellationToken) =>
        WriteAsync("recuperacao", email, userId, token, cancellationToken);

    private async Task WriteAsync(string purpose, string email, Guid userId, string token, CancellationToken cancellationToken)
    {
        string directory = Path.Combine(environment.ContentRootPath, ".dev-emails");
        Directory.CreateDirectory(directory);
        string file = Path.Combine(directory, $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.txt");
        string body = $"Tipo: {purpose}{Environment.NewLine}E-mail: {email}{Environment.NewLine}UserId: {userId}{Environment.NewLine}Token: {token}{Environment.NewLine}";
        await File.WriteAllTextAsync(file, body, Encoding.UTF8, cancellationToken);
    }
}
