using System.Collections.Concurrent;
using System.Security.Claims;
using ChatApp.Api.DTOs;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ChatService    _chats;
    private readonly MessageService _messages;
    private readonly UserService    _users;
    private readonly StatusService  _statuses;

    // connectionId -> userId
    private static readonly ConcurrentDictionary<string, Guid> _connections = new();

    public ChatHub(ChatService c, MessageService m, UserService u, StatusService s)
        => (_chats, _messages, _users, _statuses) = (c, m, u, s);

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _connections[Context.ConnectionId] = userId;
        await _users.SetOnlineAsync(userId, true);

        var chatIds = await _chats.GetUserChatIdsAsync(userId);
        foreach (var id in chatIds)
            await Groups.AddToGroupAsync(Context.ConnectionId, id.ToString());

        await Clients.Others.SendAsync("UserOnline", userId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        var userId = GetUserId();
        _connections.TryRemove(Context.ConnectionId, out _);

        // Only set offline if no other connections exist for this user
        if (!_connections.Values.Any(id => id == userId))
        {
            var lastSeen = DateTime.UtcNow;
            await _users.SetOnlineAsync(userId, false, lastSeen);
            await Clients.Others.SendAsync("UserOffline", userId, lastSeen);
        }

        await base.OnDisconnectedAsync(ex);
    }

    public async Task SendMessage(SendMessageRequest req)
    {
        var userId = GetUserId();
        if (!await _chats.CanSendMessageAsync(req.ChatId, userId))
        {
            await Clients.Caller.SendAsync("Error", "No permission to send here.");
            return;
        }
        var msg = await _messages.CreateAsync(req, userId);
        await Clients.Group(req.ChatId.ToString())
            .SendAsync("ReceiveMessage", MessageDto.From(msg, userId));
    }

    public async Task EditMessage(Guid messageId, string newContent)
    {
        var msg = await _messages.EditAsync(messageId, GetUserId(), newContent);
        if (msg == null) return;
        await Clients.Group(msg.ChatId.ToString())
            .SendAsync("MessageEdited", MessageDto.From(msg, GetUserId()));
    }

    public async Task DeleteMessage(Guid messageId)
    {
        var msg = await _messages.DeleteAsync(messageId, GetUserId());
        if (msg == null) return;
        await Clients.Group(msg.ChatId.ToString())
            .SendAsync("MessageDeleted", messageId, msg.ChatId);
    }

    public async Task ReactToMessage(Guid messageId, string emoji)
    {
        var reactions = await _messages.ToggleReactionAsync(messageId, GetUserId(), emoji);
        var chatId    = await _messages.GetChatIdAsync(messageId);
        if (chatId == Guid.Empty) return;
        await Clients.Group(chatId.ToString())
            .SendAsync("ReactionsUpdated", messageId, reactions);
    }

    public async Task TypingIndicator(Guid chatId, bool isTyping) =>
        await Clients.OthersInGroup(chatId.ToString())
            .SendAsync("UserTyping", chatId, GetUserId(), isTyping);

    public async Task MarkAsRead(Guid chatId)
    {
        var userId = GetUserId();
        var readAt = DateTime.UtcNow;
        await _chats.UpdateLastReadAsync(chatId, userId, readAt);
        await Clients.Group(chatId.ToString())
            .SendAsync("MessagesRead", chatId, userId, readAt);
    }

    public async Task BroadcastNewStatus(Guid statusId)
    {
        var userId  = GetUserId();
        var partnerIds = await _chats.GetDirectPartnerIdsAsync(userId);
        var connIds = _connections
            .Where(kv => partnerIds.Contains(kv.Value))
            .Select(kv => kv.Key)
            .ToList();
        if (connIds.Any())
            await Clients.Clients(connIds).SendAsync("NewStatus", userId, statusId);
    }

    public async Task JoinChatGroup(Guid chatId) =>
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

    public async Task LeaveChatGroup(Guid chatId) =>
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());

    private Guid GetUserId() =>
        Guid.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
