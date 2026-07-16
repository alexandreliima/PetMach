using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PetMach.Application.Chat;

namespace PetMach.Api.Hubs;

[Authorize]
public sealed class ChatHub(IChatService chat) : Hub
{
    public async Task JoinConversation(Guid conversationId)
    {
        if (!Guid.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirstValue("sub"), out Guid userId) ||
            !await chat.CanAccessAsync(userId, conversationId, Context.ConnectionAborted))
            throw new HubException("Conversa indisponível.");
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(conversationId), Context.ConnectionAborted);
    }

    public static string GroupName(Guid conversationId) => $"conversation:{conversationId:N}";
}
