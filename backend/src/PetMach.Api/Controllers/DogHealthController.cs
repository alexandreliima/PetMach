using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Health;
using PetMach.Contracts.Health;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/dogs/{dogId:guid}/health")]
public sealed class DogHealthController(IDogHealthService health) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(Guid dogId, CancellationToken cancellationToken)
    {
        Result<DogHealthResponse> result = await health.GetAsync(UserId(), dogId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Error(result.Error);
    }

    [HttpPost("vaccinations")]
    public async Task<IActionResult> Vaccination(Guid dogId, CreateVaccinationRequest request, CancellationToken cancellationToken)
    {
        Result<VaccinationResponse> result = await health.AddVaccinationAsync(UserId(), dogId, request, cancellationToken);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : Error(result.Error);
    }

    [HttpPost("vaccinations/{vaccinationId:guid}/proof")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProof(Guid dogId, Guid vaccinationId, IFormFile file, CancellationToken cancellationToken)
    {
        await using Stream stream = file.OpenReadStream();
        Result<VaccinationProofResponse> result = await health.AddVaccinationProofAsync(UserId(), dogId, vaccinationId, stream, file.Length, cancellationToken);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : Error(result.Error);
    }

    [HttpGet("vaccinations/{vaccinationId:guid}/proof")]
    public async Task<IActionResult> DownloadProof(Guid dogId, Guid vaccinationId, CancellationToken cancellationToken)
    {
        Result<ProtectedHealthFile> result = await health.GetVaccinationProofAsync(UserId(), dogId, vaccinationId, cancellationToken);
        return result.IsSuccess ? File(result.Value.Content, result.Value.ContentType, result.Value.FileName) : Error(result.Error);
    }

    [HttpPost("dewormings")]
    public async Task<IActionResult> Deworming(Guid dogId, CreateDewormingRequest request, CancellationToken cancellationToken)
    {
        Result<DewormingResponse> result = await health.AddDewormingAsync(UserId(), dogId, request, cancellationToken);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : Error(result.Error);
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;

    private ObjectResult Error(DomainError error)
    {
        int status = error.Code.EndsWith("not_found", StringComparison.Ordinal) ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        ProblemDetails problem = new() { Status = status, Title = error.Description, Type = $"https://petmach.local/problems/{error.Code}" };
        problem.Extensions["code"] = error.Code;
        return StatusCode(status, problem);
    }
}
