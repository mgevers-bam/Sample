using Ardalis.Result;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Stargate.Core.Boundary.Events;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Core.Commands;

public class CreatePersonCommand :
    IRequest<Result<int>>,
    MassTransit.Mediator.Request<Result<int>>
{
    public required string Name { get; set; } = string.Empty;
}

public class CreatePersonRequestHandler(
    ILogger<CreatePersonRequestHandler> logger,
    IPersonRepository repository,
    IMediator mediator) : IRequestHandler<CreatePersonCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = new Person(request.Name);
        repository.Add(person);

        return await repository.CommitTransaction(cancellationToken)
            .Map(() => person.Id)
            .Tap(async () =>
            {
                logger.LogInformation("Created person {Name} with ID {Id}", person.Name, person.Id);
                await mediator.Publish(new PersonCreatedEvent() { Id = person.Id, Name = person.Name }, cancellationToken);
            })
            .TapError(error =>
            {
                logger.LogError(
                    "Failed to commit transaction for creating person {Name}: {Error}",
                    request.Name,
                    string.Join(",", error.Errors));
            });
    }
}

public class CreatePersonCommandHandler(
    ILogger<CreatePersonCommandConsumer> logger,
    IPersonRepository repository,
    IPublishEndpoint publishEndpoint) : MassTransit.Mediator.MediatorRequestHandler<CreatePersonCommand, Result<int>>
{
    protected override async Task<Result<int>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = new Person(request.Name);
        repository.Add(person);

        return await repository.CommitTransaction(cancellationToken)
            .Map(() => person.Id)
            .Tap(async () =>
            {
                logger.LogInformation("Created person {Name} with ID {Id}", person.Name, person.Id);
                await publishEndpoint.Publish(new PersonCreatedEvent() { Id = person.Id, Name = person.Name }, cancellationToken);
            })
            .TapError(error =>
            {
                logger.LogError(
                    "Failed to commit transaction for creating person {Name}: {Error}",
                    request.Name,
                    string.Join(",", error.Errors));
            });
    }
}

public class CreatePersonCommandConsumer(
    ILogger<CreatePersonCommandConsumer> logger,
    IPersonRepository repository) : IConsumer<CreatePersonCommand>
{
    public async Task Consume(ConsumeContext<CreatePersonCommand> context)
    {
        var person = new Person(context.Message.Name);
        repository.Add(person);

        var result = await repository.CommitTransaction(context.CancellationToken)
            .Map(() => person.Id)
            .Tap(async () =>
            {
                logger.LogInformation("Created person {Name} with ID {Id}", person.Name, person.Id);
                await context.Publish(new PersonCreatedEvent() { Id = person.Id, Name = person.Name }, context.CancellationToken);
            })
            .TapError(error =>
            {
                logger.LogError(
                    "Failed to commit transaction for creating person {Name}: {Error}",
                    context.Message.Name,
                    string.Join(",", error.Errors));
            });

        await context.RespondAsync(result);
    }
}
