using Ardalis.Result;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Stargate.Core.Boundary.Events;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Core.Commands;

public class CreateAstronautDutyCommand : IRequest<Result<int>>
{
    public required string Name { get; set; }

    public required string Rank { get; set; }

    public required string DutyTitle { get; set; }

    public DateTime DutyStartDate { get; set; }
}

public class CreateAstronautDutyCommandHandler(
    ILogger<CreateAstronautDutyCommandHandler> logger,
    IPersonRepository repository,
    IMediator mediator) : IRequestHandler<CreateAstronautDutyCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateAstronautDutyCommand request, CancellationToken cancellationToken)
    {
        return await repository.GetPersonByNameAsync(request.Name, cancellationToken)
            .Map(person =>
            {
                return person.AddAstronautDuty(request.Rank, request.DutyTitle, request.DutyStartDate)
                    .Bind(() =>
                    {
                        repository.Update(person);
                        return repository.CommitTransaction(cancellationToken);
                    })
                    .Tap(() =>
                    {
                        var @event = new AstronautDutyCreatedEvent()
                        {
                            PersonId = person.Id,
                            Name = request.Name,
                            Rank = request.Rank,
                            DutyTitle = request.DutyTitle,
                            DutyStartDate = request.DutyStartDate
                        };

                        return mediator.Publish(@event, cancellationToken);
                    })
                    .Map(() => person);
            })
            .Tap(person => logger.LogInformation("Created astronaut duty {duty}", person.AstronautDuties.Last()))
            .TapError(error => logger.LogError(
                "Failed to create astronaut duty for person {Name}: encountered errors {Errors}",
                request.Name,
                string.Join(",", error.Errors)))
            .Map(person => person.AstronautDuties.Last().Id);

        return await HandleVerbose(request, cancellationToken);
    }

    private async Task<Result<int>> HandleVerbose(CreateAstronautDutyCommand request, CancellationToken cancellationToken)
    {
        var personResult = await repository.GetPersonByNameAsync(request.Name, cancellationToken);

        if (!personResult.IsSuccess)
        {
            logger.LogError(
                "Failed to retreive person with name {Name}: {Error}",
                request.Name,
                string.Join(",", personResult.Errors));

            return personResult.AsTypedError<Person, int>();
        }

        var person = personResult.Value;
        var addDutyResult = person.AddAstronautDuty(
            request.Rank,
            request.DutyTitle,
            request.DutyStartDate);

        if (!addDutyResult.IsSuccess)
        {
            logger.LogError(
                "Failed to add astronaught duty for person {Name}: {Error}",
                request.Name,
                string.Join(",", addDutyResult.Errors));
            return addDutyResult.AsTypedError<int>();
        }

        repository.Update(person);
        var commitResult = await repository.CommitTransaction(cancellationToken);

        if (!commitResult.IsSuccess)
        {
            logger.LogError(
                "Failed to commit transaction for creating person {Name}: {Error}",
                request.Name,
                string.Join(",", commitResult.Errors));
            return commitResult.AsTypedError<int>();
        }

        logger.LogInformation("Created astronaut duty {duty}", person.AstronautDuties.Last());
        var @event = new AstronautDutyCreatedEvent()
        {
            PersonId = person.Id,
            Name = request.Name,
            Rank = request.Rank,
            DutyTitle = request.DutyTitle,
            DutyStartDate = request.DutyStartDate
        };
        await mediator.Publish(@event, cancellationToken);

        return Result.Success(person.AstronautDuties.Last().Id);
    }
}

public class CreateAstronautDutyCommandConsumer(
    ILogger<CreateAstronautDutyCommandConsumer> logger,
    IPersonRepository repository) : IConsumer<CreateAstronautDutyCommand>
{
    public async Task Consume(ConsumeContext<CreateAstronautDutyCommand> context)
    {
        var personResult = await repository.GetPersonByNameAsync(context.Message.Name, context.CancellationToken);

        if (!personResult.IsSuccess)
        {
            logger.LogError(
                "Failed to retreive person with name {Name}: {Error}",
                context.Message.Name,
                string.Join(",", personResult.Errors));

            await context.RespondAsync(personResult.AsTypedError<Person, int>());
            return;
        }

        var person = personResult.Value;
        var addDutyResult = person.AddAstronautDuty(
            context.Message.Rank,
            context.Message.DutyTitle,
            context.Message.DutyStartDate);

        if (!addDutyResult.IsSuccess)
        {
            logger.LogError(
                "Failed to add astronaught duty for person {Name}: {Error}",
                context.Message.Name,
                string.Join(",", addDutyResult.Errors));

            await context.RespondAsync(addDutyResult.AsTypedError<int>());
            return;
        }

        repository.Update(person);
        var commitResult = await repository.CommitTransaction(context.CancellationToken);

        if (!commitResult.IsSuccess)
        {
            logger.LogError(
                "Failed to commit transaction for creating person {Name}: {Error}",
                context.Message.Name,
                string.Join(",", commitResult.Errors));

            await context.RespondAsync(commitResult.AsTypedError<int>());
            return;
        }

        logger.LogInformation("Created astronaut duty {duty}", person.AstronautDuties.Last());
        var @event = new AstronautDutyCreatedEvent()
        {
            PersonId = person.Id,
            Name = context.Message.Name,
            Rank = context.Message.Rank,
            DutyTitle = context.Message.DutyTitle,
            DutyStartDate = context.Message.DutyStartDate
        };
        await context.Publish(@event, context.CancellationToken);

        await context.RespondAsync(Result.Success(person.AstronautDuties.Last().Id));
    }
}

