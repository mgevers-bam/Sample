using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Stargate.Core.Boundary.Events;
using Stargate.Core.Commands;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;
using Stargate.Testing;

namespace Stargate.Core.Tests.Commands;

public class CreateAstronautDutyTests_Mocks
{
    private readonly Mock<ILogger<CreateAstronautDutyCommandHandler>> _logger = new();
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task CanAddAstronautDuty()
    {
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);
        var repository = GetRepository(person, Result.Success());
        var handler = new CreateAstronautDutyCommandHandler(_logger.Object, repository.Object, _mediator.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        repository.Verify(x => x.Update(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);

        _mediator.Verify(x => x.Publish(
            It.Is<AstronautDutyCreatedEvent>(e => 
                e.PersonId == person.Id
                && e.Name == command.Name
                && e.Rank == command.Rank
                && e.DutyTitle == command.DutyTitle
                && e.DutyStartDate == command.DutyStartDate),
            It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddAstronautDuty_WhenPersonNotFound_ReturnsFailure()
    {
        var name = "James Bond";
        var repository = GetRepository(person: null, Result.Success()); 
        var handler = new CreateAstronautDutyCommandHandler(_logger.Object, repository.Object, _mediator.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = name,
            DutyTitle = "Commander",
            Rank = "Colonel",
            DutyStartDate = DateTime.UtcNow,
        };
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddAstronautDuty_WhenPersonAlreadyRetired_ReturnsFailure()
    {
        var person = DataModels.CreateAstronaut([ new AstronautDutyInfo { DutyTitle = AstronautDuty.Retired } ]);
        var duty = DataModels.CreateAstronautDuty(person);
        var repository = GetRepository(person, Result.Success());
        var handler = new CreateAstronautDutyCommandHandler(_logger.Object, repository.Object, _mediator.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddAstronautDuty_WhenSaveFails_ReturnsFailure()
    {
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);
        var repository = GetRepository(person, Result.CriticalError("could not update database"));
        var handler = new CreateAstronautDutyCommandHandler(_logger.Object, repository.Object, _mediator.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        repository.Verify(x => x.Update(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);

        Assert.False(result.IsSuccess);
    }

    private static Mock<IPersonRepository> GetRepository(
        Person? person,
        Result saveResult)
    {
        var mock = new Mock<IPersonRepository>();
        var personResult = person == null
            ? Result<Person>.NotFound($"Person with that name not found")
            : Result.Success(person);

        mock
            .Setup(x => x.GetPersonByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(personResult);

        mock
            .Setup(x => x.CommitTransaction(It.IsAny<CancellationToken>()))
            .ReturnsAsync(saveResult);

        return mock;
    }
}
