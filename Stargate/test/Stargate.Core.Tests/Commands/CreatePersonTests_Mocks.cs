using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Stargate.Core.Boundary.Events;
using Stargate.Core.Commands;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;

namespace Stargate.Core.Tests.Commands;

public class CreatePersonTests_Mocks
{
    private readonly Mock<ILogger<CreatePersonCommandHandler>> _logger = new();
    private readonly Mock<IMediator> _mediator = new();

    [Fact]
    public async Task CanAddPerson()
    {
        var repository = GetRepository(Result.Success());
        var handler = new CreatePersonCommandHandler(_logger.Object, repository.Object, _mediator.Object);

        var command = new CreatePersonCommand
        {
            Name = "James Bond"
        };
        var result = await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.Add(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);

        _mediator.Verify(
            x => x.Publish(It.Is<PersonCreatedEvent>(e => e.Id == 0 && e.Name == command.Name),
            It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddPerson_WhenSaveFails_ReturnsFailure()
    {
        var conflictMessage = "A Person with the name already exists";
        var repository = GetRepository(Result.Conflict(conflictMessage));
        var handler = new CreatePersonCommandHandler(_logger.Object, repository.Object, _mediator.Object);

        var command = new CreatePersonCommand
        {
            Name = "James Bond"
        };
        var result = await handler.Handle(command, CancellationToken.None);

        repository.Verify(x => x.Add(It.IsAny<Person>()), Times.Once);
        repository.Verify(x => x.CommitTransaction(It.IsAny<CancellationToken>()), Times.Once);

        Assert.False(result.IsSuccess);
    }

    private static Mock<IPersonRepository> GetRepository(Result result)
    {
        var mock = new Mock<IPersonRepository>();

        mock
            .Setup(x => x.CommitTransaction(It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return mock;
    }
}
