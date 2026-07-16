using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PetMach.Api.Hubs;
using PetMach.Application.Chat;
using PetMach.Contracts.Chat;
using PetMach.Domain.SharedKernel;

namespace PetMach.Api.Controllers;

[ApiController]
[Authorize(Policy = "TutorAccess")]
[Route("api/v1/chat/conversations")]
public sealed class ChatController(IChatService chat, IHubContext<ChatHub> hub) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyCollection<ConversationResponse>> List(CancellationToken cancellationToken) =>
        chat.ListAsync(UserId(), cancellationToken);

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> History(Guid conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken cancellationToken = default)
    {
        Result<ChatMessagePageResponse> result = await chat.HistoryAsync(UserId(), conversationId, page, pageSize, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<IActionResult> Send(Guid conversationId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        Result<ChatMessageResponse> result = await chat.SendAsync(UserId(), conversationId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            int status = result.Error.Code == "chat.invalid_message" ? StatusCodes.Status400BadRequest : StatusCodes.Status404NotFound;
            return StatusCode(status, new ProblemDetails { Status = status, Title = result.Error.Description });
        }
        await hub.Clients.Group(ChatHub.GroupName(conversationId)).SendAsync("MessageReceived", result.Value, cancellationToken);
        return Created($"/api/v1/chat/conversations/{conversationId}/messages/{result.Value.Id}", result.Value);
    }

    [HttpPut("{conversationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid conversationId, MarkConversationReadRequest request, CancellationToken cancellationToken)
    {
        Result<ConversationReadResponse> result = await chat.MarkReadAsync(UserId(), conversationId, request, cancellationToken);
        if (!result.IsSuccess) return NotFound();
        await hub.Clients.Group(ChatHub.GroupName(conversationId)).SendAsync("ConversationRead", result.Value, cancellationToken);
        return Ok(result.Value);
    }

    private Guid UserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out Guid id) ? id : Guid.Empty;
}
