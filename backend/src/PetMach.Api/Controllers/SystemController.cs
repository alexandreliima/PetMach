using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using PetMach.Contracts.System;

namespace PetMach.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/system")]
public sealed class SystemController(TimeProvider timeProvider, IHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    [OutputCache(Duration = 30)]
    [ProducesResponseType<SystemInfoResponse>(StatusCodes.Status200OK)]
    public ActionResult<SystemInfoResponse> Get()
    {
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        return Ok(new SystemInfoResponse(
            "PetMach.Api",
            version,
            environment.EnvironmentName,
            timeProvider.GetUtcNow()));
    }
}
