using MassTransit;
using MediatR;
using Stargate.Core.Boundary.Events;

namespace Stargate.Api.EventHandlers;

public class TranslateNotificationsToServerSentEvents(IMediator mediator) :
    INotificationHandler<PersonCreatedEvent>,
    INotificationHandler<AstronautDutyCreatedEvent>
{
    public Task Handle(PersonCreatedEvent request, CancellationToken cancellationToken)
    {
        var sseEvent = ServerSentEventDetails.ToAll(request);

        return mediator.Publish(sseEvent, cancellationToken);
    }

    public Task Handle(AstronautDutyCreatedEvent request, CancellationToken cancellationToken)
    {
        var sseEvent = ServerSentEventDetails.ToAll(request);

        return mediator.Publish(sseEvent, cancellationToken);
    }
}

public class TranslateEventsToServerSentEvents() :
    IConsumer<PersonCreatedEvent>,
    IConsumer<AstronautDutyCreatedEvent>
{
    public Task Consume(ConsumeContext<PersonCreatedEvent> context)
    {
        var sseEvent = ServerSentEventDetails.ToAll(context.Message);

        return context.Publish(sseEvent, context.CancellationToken);
    }

    public Task Consume(ConsumeContext<AstronautDutyCreatedEvent> context)
    {
        var sseEvent = ServerSentEventDetails.ToAll(context.Message);

        return context.Publish(sseEvent, context.CancellationToken);
    }
}
