namespace PetMach.Infrastructure.Identity;

internal interface ITransactionalEmailSender
{
    Task SendEmailConfirmationAsync(string email, Guid userId, string token, CancellationToken cancellationToken);
    Task SendPasswordResetAsync(string email, Guid userId, string token, CancellationToken cancellationToken);
}
