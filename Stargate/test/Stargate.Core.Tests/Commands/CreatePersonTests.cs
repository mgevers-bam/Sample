using Ardalis.Result;
using Microsoft.Extensions.Logging;
using Moq;
using Stargate.Core.Commands;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;
using Stargate.Testing.Logging;

namespace Stargate.Core.Tests.Commands;

public class CreatePersonTests
{
    [Fact]
    public async Task CanAddPerson()
    {
        var logger = GetLogger();
        var repository = GetRepository(Result.Success());
        var handler = new CreatePersonCommandHandler(logger.Object, repository.Object);

        var command = new CreatePersonCommand
        {
            Name = "James Bond"
        };
        var result = await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.Add(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);
        // This is logging as ID 0 because nothing in repository is setting the ID like in a real database
        logger.AssertLogs(new LogEntry(LogLevel.Information, "Created person James Bond with ID 0"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddPerson_WhenSaveFails_ReturnsFailure()
    {
        var logger = GetLogger();
        var conflictMessage = "A Person with the name already exists";
        var repository = GetRepository(Result.Conflict(conflictMessage));
        var handler = new CreatePersonCommandHandler(logger.Object, repository.Object);

        var command = new CreatePersonCommand
        {
            Name = "James Bond"
        };
        var result = await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.Add(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);
        logger.AssertLogs(new LogEntry(LogLevel.Error, $"Failed to commit transaction for creating person {command.Name}: {conflictMessage}"));

        Assert.False(result.IsSuccess);
    }

    private static Mock<ILogger<CreatePersonCommandHandler>> GetLogger()
    {
        return new Mock<ILogger<CreatePersonCommandHandler>>();
    }

    private static Mock<IRepository<Person>> GetRepository(Result result)
    {
        var mock = new Mock<IRepository<Person>>();

        mock
            .Setup(x => x.CommitTransaction(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return mock;
    }
}
