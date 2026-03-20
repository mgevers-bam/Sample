using MediatR;
using Stargate.Infrastructure.ServerSentEvents;

namespace Stargate.Api;

public class ServerSentEventDetails : INotification
{
    public static ServerSentEventDetails ToAll(object @event)
    {
        return new ServerSentEventDetails(@event)
        {
            SendType = ClientSendType.All,
        };
    }

    public static ServerSentEventDetails ToUser(Guid userId, object @event)
    {
        return new ServerSentEventDetails(@event)
        {
            SendType = ClientSendType.User,
            UserId = userId
        };
    }

    public static ServerSentEventDetails ToGroups(IReadOnlyList<string> groups, object @event)
    {
        return new ServerSentEventDetails(@event)
        {
            SendType = ClientSendType.Groups,
            Groups = groups
        };
    }

    protected ServerSentEventDetails(object @event)
    {
        Event = new ServerEvent(@event);
    }

    protected ServerSentEventDetails()
    {
    }

    public ClientSendType SendType { get; private set; }
    public Guid? UserId { get; private set; }
    public IReadOnlyList<string> Groups { get; private set; } = [];

    public ServerEvent Event { get; private set; } = null!;
}

public enum ClientSendType
{
    All,
    User,
    Groups,
}
