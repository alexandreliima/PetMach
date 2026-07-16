using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Identity;
using PetMach.Contracts.Identity;
using PetMach.Domain.Identity;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IIdentityService identity) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<RegistrationResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        Result<RegistrationResponse> result = await identity.RegisterAsync(request, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToProblem(result.Error);
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await identity.ConfirmEmailAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        Result<TokenResponse> result = await identity.LoginAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        Result<TokenResponse> result = await identity.RefreshAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await identity.LogoutAsync(CurrentUserId(), request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await identity.RequestPasswordResetAsync(request, cancellationToken);
        return Accepted(new { message = "Se o e-mail estiver cadastrado, as instruções serão enviadas." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await identity.ResetPasswordAsync(request, cancellationToken));

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        Result<AccountResponse> result = await identity.GetAccountAsync(CurrentUserId(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToProblem(result.Error);
    }

    [Authorize]
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await identity.AnonymizeAccountAsync(CurrentUserId(), request, cancellationToken));

    private Guid CurrentUserId()
    {
        string? value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out Guid userId) ? userId : Guid.Empty;
    }

    private IActionResult ToActionResult(Result result) => result.IsSuccess ? NoContent() : ToProblem(result.Error);

    private ObjectResult ToProblem(DomainError error)
    {
        int status = error.Code switch
        {
            "identity.invalid_credentials" => StatusCodes.Status401Unauthorized,
            "identity.refresh_token_reuse" => StatusCodes.Status401Unauthorized,
            "identity.email_not_confirmed" => StatusCodes.Status403Forbidden,
            "identity.account_unavailable" => StatusCodes.Status403Forbidden,
            "identity.email_already_registered" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };
        ProblemDetails problem = new()
        {
            Status = status,
            Title = error.Description,
            Type = $"https://petmach.local/problems/{error.Code}",
        };
        problem.Extensions["code"] = error.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return StatusCode(status, problem);
    }
}
