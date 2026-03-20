using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Stargate.Core.Commands;

namespace Stargate.Api.Hubs;

[Authorize]
public class ServerEventHub(ILogger<ServerEventHub> logger, IPublishEndpoint publishEndpoint) : Hub<IHubClient>
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        if (this.Context.User?.Identity?.IsAuthenticated == true)
        {
            var claims = this.Context.User.Claims
                .Select(c => new { c.Type, c.Value })
                .ToList();
            logger.LogInformation("Authenticated client connected: {ConnectionId}, User-Claims: {User-Claims}", Context.ConnectionId, claims);
        }
        else
        {
            logger.LogInformation("Unauthenticated client connected: {ConnectionId}", Context.ConnectionId);
        }
    }

    public async Task CreatePerson(CreatePersonCommand command)
    {
        await publishEndpoint.Publish(command);
    }
}
