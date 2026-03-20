using Stargate.Infrastructure.ServerSentEvents;

namespace Stargate.Api.Hubs;

public interface IHubClient
{
    Task PushServerEvent(ServerEvent serverEvent, CancellationToken cancellationToken);
}
