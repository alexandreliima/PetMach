using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Dogs;
using PetMach.Contracts.Dogs;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController, Authorize(Policy = "TutorAccess"), Route("api/v1/dogs/{dogId:guid}/photos")]
public sealed class DogMediaController(IDogPhotoService photos) : ControllerBase
{
    [HttpGet] public Task<IReadOnlyCollection<DogPhotoResponse>> List(Guid dogId, CancellationToken ct) => photos.ListAsync(UserId(), dogId, ct);
    [HttpPost, RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Upload(Guid dogId, IFormFile file, CancellationToken ct)
    {
        await using Stream stream = file.OpenReadStream(); Result<DogPhotoResponse> result = await photos.AddAsync(UserId(), dogId, stream, file.ContentType, file.Length, ct);
        if (result.IsSuccess) return StatusCode(201, result.Value);
        ProblemDetails problem = new() { Status = 400, Title = result.Error.Description, Type = $"https://petmach.local/problems/{result.Error.Code}" }; problem.Extensions["code"] = result.Error.Code; return BadRequest(problem);
    }
    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
