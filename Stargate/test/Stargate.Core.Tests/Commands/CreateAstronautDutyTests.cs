using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Moq;
using Stargate.Core.Commands;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;
using Stargate.Testing;
using Stargate.Testing.Logging;

namespace Stargate.Core.Tests.Commands;

public class CreateAstronautDutyTests
{
    [Fact]
    public async Task CanAddAstronautDuty()
    {
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);
        var logger = GetLogger();
        var repository = GetRepository(person, Result.Success());
        var handler = new CreateAstronautDutyCommandHandler(logger.Object, repository.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };
        var result = await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.Update(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);
        logger.AssertLogs(new LogEntry(LogLevel.Information, $"Created astronaut duty {duty}"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddAstronautDuty_WhenPersonNotFound_ReturnsFailure()
    {
        var name = "James Bond";
        var logger = GetLogger();
        var repository = GetRepository(person: null, Result.Success());
        var handler = new CreateAstronautDutyCommandHandler(logger.Object, repository.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = name,
            DutyTitle = "Commander",
            Rank = "Colonel",
            DutyStartDate = DateTime.UtcNow,
        };
        var result = await handler.Handle(command, CancellationToken.None);

        logger.AssertLogs(new LogEntry(LogLevel.Error, $"Failed to retreive person with name {name}: Person with that name not found"));
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddAstronautDuty_WhenPersonAlreadyRetired_ReturnsFailure()
    {
        var person = DataModels.CreateAstronaut([ new AstronautDutyInfo { DutyTitle = AstronautDuty.Retired } ]);
        var duty = DataModels.CreateAstronautDuty(person);
        var logger = GetLogger();
        var repository = GetRepository(person, Result.Success());
        var handler = new CreateAstronautDutyCommandHandler(logger.Object, repository.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };
        var result = await handler.Handle(command, CancellationToken.None);

        logger.AssertLogs(new LogEntry(LogLevel.Error, $"Failed to add astronaught duty for person {person.Name}: Cannot add a new duty after retirement."));
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task AddAstronautDuty_WhenSaveFails_ReturnsFailure()
    {
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);
        var logger = GetLogger();
        var repository = GetRepository(person, Result.CriticalError("could not update database"));
        var handler = new CreateAstronautDutyCommandHandler(logger.Object, repository.Object);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };
        var result = await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.Update(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);
        logger.AssertLogs(new LogEntry(LogLevel.Error, $"Failed to commit transaction for creating person {person.Name}: could not update database"));

        Assert.False(result.IsSuccess);
    }

    private static Mock<ILogger<CreateAstronautDutyCommandHandler>> GetLogger()
    {
        return new Mock<ILogger<CreateAstronautDutyCommandHandler>>();
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
