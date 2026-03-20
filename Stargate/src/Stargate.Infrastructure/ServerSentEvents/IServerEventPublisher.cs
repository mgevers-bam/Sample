using System.Threading.Channels;

namespace Stargate.Infrastructure.ServerSentEvents;

public interface IServerEventPublisher
{
    Guid Connect();
    void Disconnect(Guid connectionId);

    void AddConnectionToGroup(Guid connectionId, string groupName);
    void RemoveConnectionFromGroup(Guid connectionId, string groupName);

    Task PublishToAll<T>(T @event, CancellationToken cancellationToken = default)
        where T : ServerEvent;

    Task PublishToUser<T>(Guid userId, T @event, CancellationToken cancellationToken = default)
        where T : ServerEvent;

    Task PublishToGroup<T>(string groupName, T @event, CancellationToken cancellationToken = default)
        where T : ServerEvent;

    ChannelReader<ServerEvent> GetEventStream(Guid connectionId);
}
