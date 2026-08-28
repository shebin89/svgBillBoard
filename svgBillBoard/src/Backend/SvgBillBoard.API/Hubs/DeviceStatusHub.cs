using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SvgBillBoard.API.Hubs;

[Authorize]
public class DeviceStatusHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var organizationId =
            Context.User?
                .FindFirst("organizationId")?
                .Value;

        if (!string.IsNullOrWhiteSpace(organizationId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"organization:{organizationId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(
        Exception? exception)
    {
        var organizationId =
            Context.User?
                .FindFirst("organizationId")?
                .Value;

        if (!string.IsNullOrWhiteSpace(organizationId))
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"organization:{organizationId}");
        }

        await base.OnDisconnectedAsync(exception);
 
    }
}