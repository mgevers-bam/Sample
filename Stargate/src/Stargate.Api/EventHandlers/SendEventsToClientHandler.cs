using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Stargate.Api.Hubs;
using Stargate.Infrastructure.ServerSentEvents;

namespace Stargate.Api.EventHandlers;

public class SendNotificationsToClientHandler(IServerEventPublisher eventPublisher, IHubContext<ServerEventHub, IHubClient> hubContext) :
    INotificationHandler<ServerSentEventDetails>
{
    public async Task Handle(ServerSentEventDetails request, CancellationToken cancellationToken)
    {
        var @event = request.Event;

        switch (request.SendType)
        {
            case ClientSendType.All:
                await eventPublisher.PublishToAll(@event, cancellationToken);
                await hubContext.Clients.All
                    .PushServerEvent(@event, cancellationToken);
                break;
            case ClientSendType.User:
                await eventPublisher.PublishToUser(request.UserId!.Value, @event, cancellationToken);
                await hubContext.Clients
                    .User(request.UserId.Value.ToString())
                    .PushServerEvent(@event, cancellationToken);
                break;
            case ClientSendType.Groups:
                var tasks = request.Groups
                    .SelectMany(group => 
                    {
                        return new Task[]
                        {
                            eventPublisher.PublishToGroup(group, @event, cancellationToken),
                            hubContext.Clients
                                .Group(group)
                                .PushServerEvent(@event, cancellationToken),
                        };
                    })
                    .ToList();

                await Task.WhenAll(tasks);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.SendType), "Invalid send type.");
        }
    }
}

public class SendEventsToClientHandler(IServerEventPublisher eventPublisher, IHubContext<ServerEventHub, IHubClient> hubContext)
    : IConsumer<ServerSentEventDetails>
{
    public async Task Consume(ConsumeContext<ServerSentEventDetails> context)
    {
        var @event = context.Message.Event;

        switch (context.Message.SendType)
        {
            case ClientSendType.All:
                await eventPublisher.PublishToAll(@event, context.CancellationToken);
                await hubContext.Clients.All.PushServerEvent(@event, context.CancellationToken);
                break;
            case ClientSendType.User:
                await eventPublisher.PublishToUser(context.Message.UserId!.Value, @event, context.CancellationToken);
                await hubContext.Clients
                    .User(context.Message.UserId.Value.ToString())
                    .PushServerEvent(@event, context.CancellationToken);

                break;
            case ClientSendType.Groups:
                var tasks = context.Message.Groups
                    .SelectMany(group =>
                    {
                        return new Task[]
                        {
                            eventPublisher.PublishToGroup(group, @event, context.CancellationToken),
                            hubContext.Clients
                                .Group(group)
                                .PushServerEvent(@event, context.CancellationToken),
                        };
                    }).ToList();

                await Task.WhenAll(tasks);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context.Message.SendType), "Invalid send type.");
        }
    }
}
