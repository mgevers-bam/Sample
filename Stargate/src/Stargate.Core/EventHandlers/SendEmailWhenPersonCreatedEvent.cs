using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Stargate.Core.Boundary.Events;

namespace Stargate.Core.EventHandlers;

public class SendEmailWhenPersonCreatedEvent(ILogger<SendEmailWhenPersonCreatedEvent> logger) :
    IConsumer<PersonCreatedEvent>,
    INotificationHandler<PersonCreatedEvent>
{
    public Task Consume(ConsumeContext<PersonCreatedEvent> context)
    {
        return SendEmail(context.Message, context.CancellationToken);
    }

    public Task Handle(PersonCreatedEvent notification, CancellationToken cancellationToken)
    {
        return SendEmail(notification, cancellationToken);
    }

    private async Task SendEmail(PersonCreatedEvent personCreatedEvent, CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);

        logger.LogInformation(
            "Email sent for person with Id: {Id} and Name: {Name}",
            personCreatedEvent.Id,
            personCreatedEvent.Name);
    }
}
