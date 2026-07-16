using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetMach.Application.Dogs;
using PetMach.Contracts.Dogs;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController, Authorize(Policy = "TutorAccess"), Route("api/v1/dogs")]
public sealed class DogsController(IDogService dogs) : ControllerBase
{
    private static readonly string[] BreedNames = ["Sem raça definida", "Labrador Retriever", "Golden Retriever", "Pastor Alemão", "Poodle", "Bulldog Francês", "Beagle", "Shih-tzu", "Yorkshire Terrier", "Border Collie"];
    [HttpGet] public async Task<IReadOnlyCollection<DogResponse>> List(CancellationToken ct) => await dogs.ListAsync(UserId(), ct);
    [HttpGet("{id:guid}")] public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Result(await dogs.GetAsync(UserId(), id, ct));
    [HttpPost] public async Task<IActionResult> Create(UpsertDogRequest request, CancellationToken ct) { Result<DogResponse> r = await dogs.CreateAsync(UserId(), request, ct); return r.IsSuccess ? StatusCode(201, r.Value) : Problem(r.Error, 400); }
    [HttpPut("{id:guid}")] public async Task<IActionResult> Update(Guid id, UpsertDogRequest request, CancellationToken ct) => Result(await dogs.UpdateAsync(UserId(), id, request, ct));
    [HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id, CancellationToken ct) { Result r = await dogs.DeleteAsync(UserId(), id, ct); return r.IsSuccess ? NoContent() : Problem(r.Error, 404); }
    [AllowAnonymous, HttpGet("breeds")] public ActionResult<IReadOnlyCollection<BreedResponse>> Breeds() => Ok(BreedNames.Select(x => new BreedResponse(x)));
    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
    private IActionResult Result(Result<DogResponse> result) => result.IsSuccess ? Ok(result.Value) : Problem(result.Error, 404);
    private ObjectResult Problem(DomainError error, int status) { ProblemDetails p = new() { Status = status, Title = error.Description, Type = $"https://petmach.local/problems/{error.Code}" }; p.Extensions["code"] = error.Code; return StatusCode(status, p); }
}
