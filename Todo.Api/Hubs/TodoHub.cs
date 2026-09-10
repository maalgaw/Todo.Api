using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Todo.Api.Hubs;

[Authorize]
public class TodoHub : Hub
{
    public override Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // The default IUserIdProvider uses ClaimTypes.NameIdentifier
            // SignalR automatically handles sending to user via Clients.User(userId)
        }
        return base.OnConnectedAsync();
    }
}
