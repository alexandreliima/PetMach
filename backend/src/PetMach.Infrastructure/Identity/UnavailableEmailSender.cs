namespace PetMach.Infrastructure.Identity;

internal sealed class UnavailableEmailSender : ITransactionalEmailSender
{
    public Task SendEmailConfirmationAsync(string email, Guid userId, string token, CancellationToken cancellationToken) => Throw();
    public Task SendPasswordResetAsync(string email, Guid userId, string token, CancellationToken cancellationToken) => Throw();

    private static Task Throw() => throw new InvalidOperationException(
        "Um provedor de e-mail transacional deve ser configurado fora de Development.");
}
