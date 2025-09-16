using System;
using Microsoft.AspNetCore.SignalR;

namespace StatusAPI.Hubs;

public class StatusHub : Hub
{
    public async Task JoinStatusGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "StatusUpdates");
    }

    public async Task LeaveStatusGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "StatusUpdates");
    }

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "StatusUpdates");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "StatusUpdates");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task StatusViewed(int statusId, string viewerUserId, string viewerUserName)
    {
        await Clients.Group("StatusUpdates").SendAsync("StatusViewed", new
        {
            statusId,
            viewerUserId,
            viewerUserName,
            viewedAt = DateTime.UtcNow
        });
    }
}