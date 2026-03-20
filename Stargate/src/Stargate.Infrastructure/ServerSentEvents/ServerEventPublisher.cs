using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Stargate.Infrastructure.ServerSentEvents;

public class ServerEventPublisher : IServerEventPublisher
{
    private readonly ConcurrentDictionary<Guid, Channel<ServerEvent>> _connections = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<Guid>> _groups = new();

    public Guid Connect()
    {
        var connectionId = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<ServerEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });
        _connections.TryAdd(connectionId, channel);
        return connectionId;
    }

    public void Disconnect(Guid connectionId)
    {
        if (_connections.TryRemove(connectionId, out var channel))
        {
            channel.Writer.Complete();
        }
    }

    public void AddConnectionToGroup(Guid connectionId, string groupName)
    {
        _groups.AddOrUpdate(groupName,
            _ => new ConcurrentQueue<Guid>(new[] { connectionId }),
            (_, queue) =>
            {
                queue.Enqueue(connectionId);
                return queue;
            });
    }

    public void RemoveConnectionFromGroup(Guid connectionId, string groupName)
    {
        if (_groups.TryGetValue(groupName, out var queue))
        {
            queue.TryDequeue(out _);
        }
    }

    public async Task PublishToAll<T>(T @event, CancellationToken cancellationToken = default)
        where T : ServerEvent
    {
        foreach (var connection in _connections.Values)
        {
            await connection.Writer.WriteAsync(@event, cancellationToken);
        }
    }

    public Task PublishToUser<T>(Guid userId, T @event, CancellationToken cancellationToken = default)
        where T : ServerEvent
    {
        throw new NotImplementedException();
    }

    public async Task PublishToGroup<T>(string groupName, T @event, CancellationToken cancellationToken = default)
        where T : ServerEvent
    {
        var connections = _groups.GetValueOrDefault(groupName);

        foreach (var connection in connections ?? [])
        {
            if (_connections.TryGetValue(connection, out var channel))
            {
                await channel.Writer.WriteAsync(@event, cancellationToken);
            }
        }
    }

    public ChannelReader<ServerEvent> GetEventStream(Guid connectionId)
    {
        if (_connections.TryGetValue(connectionId, out var channel))
        {
            return channel.Reader;
        }

        throw new InvalidOperationException($"Connection {connectionId} not found");
    }
}
