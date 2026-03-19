using System.Collections.Concurrent;
using System.Security.Claims;
using CollabPaint.API.DTOs;
using CollabPaint.API.Models;
using CollabPaint.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CollabPaint.API.Hubs;

[Authorize]
public class PaintHub : Hub
{
    private readonly ISessionService      _sessions;
    private readonly UserManager<AppUser> _users;

    private static readonly ConcurrentDictionary<string, HashSet<string>> _connSessions = new();

    private static readonly string[] Colors =
        ["#e63946","#2a9d8f","#e9c46a","#264653","#f4a261",
         "#6a4c93","#1982c4","#ff595e","#6a994e","#bc4749"];

    public PaintHub(ISessionService sessions, UserManager<AppUser> users)
    { _sessions = sessions; _users = users; }

    // ── Identity helpers ─────────────────────────────────────────────────────

    private string UserId =>
        Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? Context.User?.FindFirstValue("sub")
        ?? throw new HubException("Not authenticated.");

    private string UserName =>
        Context.User?.FindFirstValue(ClaimTypes.Name)
        ?? Context.User?.FindFirstValue("unique_name")
        ?? "Unknown";

    private string ColorFor(string uid) =>
        Colors[Math.Abs(uid.GetHashCode()) % Colors.Length];

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        if (_connSessions.TryRemove(Context.ConnectionId, out var sessions))
        {
            foreach (var sid in sessions)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, sid);
                await Clients.Group(sid).SendAsync("UserLeft", UserId);
                if (Guid.TryParse(sid, out var g))
                    await _sessions.RemoveParticipantAsync(g, UserId);
            }
        }
        await base.OnDisconnectedAsync(ex);
    }

    // ── Session ───────────────────────────────────────────────────────────────

    public async Task JoinSession(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        _connSessions.AddOrUpdate(Context.ConnectionId,
            _ => [sessionId],
            (_, h) => { h.Add(sessionId); return h; });

        if (Guid.TryParse(sessionId, out var g))
            await _sessions.AddParticipantAsync(g, UserId);

        var user = await _users.FindByIdAsync(UserId);
        await Clients.OthersInGroup(sessionId).SendAsync("UserJoined",
            new UserJoinedPayload(UserId, UserName, user?.DisplayName ?? UserName, ColorFor(UserId)));
    }

    public async Task LeaveSession(string sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
        if (_connSessions.TryGetValue(Context.ConnectionId, out var h)) h.Remove(sessionId);
        if (Guid.TryParse(sessionId, out var g))
            await _sessions.RemoveParticipantAsync(g, UserId);
        await Clients.Group(sessionId).SendAsync("UserLeft", UserId);
    }

    // ── Drawing ───────────────────────────────────────────────────────────────

    public async Task SendStroke(string sessionId, StrokeDataDto stroke)
    {
        var withUser = stroke with { UserId = UserId };
        if (Guid.TryParse(sessionId, out var g))
            await _sessions.SaveStrokeAsync(g, UserId, withUser);
        await Clients.OthersInGroup(sessionId).SendAsync("ReceiveStroke", withUser);
    }

    public async Task SendClear(string sessionId) =>
        await Clients.OthersInGroup(sessionId).SendAsync("ReceiveClear");

    public async Task SendCursorMove(string sessionId, double x, double y) =>
        await Clients.OthersInGroup(sessionId).SendAsync("ReceiveCursorMove",
            new CursorPayload(UserId, UserName, x, y, ColorFor(UserId)));

    // ── Invites ───────────────────────────────────────────────────────────────

    public async Task InviteUser(string targetUserId, string sessionId)
    {
        var session = Guid.TryParse(sessionId, out var g) ? await _sessions.GetByIdAsync(g) : null;
        var inviter = await _users.FindByIdAsync(UserId);
        await Clients.User(targetUserId).SendAsync("InviteReceived",
            new InvitePayload(UserId, inviter?.DisplayName ?? UserName, sessionId, session?.Name ?? "Untitled"));
    }

    public async Task AcceptInvite(string sessionId)
    {
        await JoinSession(sessionId);
        await Clients.Group(sessionId).SendAsync("InviteAccepted", UserId, UserName);
    }

    public async Task DeclineInvite(string sessionId) =>
        await Clients.Group(sessionId).SendAsync("InviteDeclined", UserId);
}
