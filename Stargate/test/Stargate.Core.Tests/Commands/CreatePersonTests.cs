using Ardalis.Result;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using Common.Testing.FluentTesting;
using Common.Testing.FluentTesting.Asserts;
using Common.Testing.Persistence;
using Stargate.Core.Boundary.Events;
using Stargate.Core.Commands;
using Stargate.Core.Contracts;
using Stargate.Core.Tests.Fakes;
using Stargate.Testing;

namespace Stargate.Core.Tests.Commands;

public class CreatePersonTests
{
    [Fact]
    public async Task CanAddPerson()
    {
        var person = DataModels.CreatePerson("James Bond");
        var command = new CreatePersonCommand
        {
            Name = person.Name,
        };

        await Arrange(DatabaseState.Empty)
            .Handle(command)
            .AssertDatabase(new DatabaseState(person))
            .AssertOutput(Result.Success(0))
            .AssertPublishedEvent(new PersonCreatedEvent() { Id = person.Id, Name = person.Name });
    }

    [Fact]
    public async Task AddPerson_WhenSaveFails_ReturnsFailure()
    {
        var dbError = Result.Conflict("A Person with the name already exists");
        var person = DataModels.CreatePerson("James Bond");
        var command = new CreatePersonCommand
        {
            Name = person.Name,
        };

        await Arrange(DatabaseState.Empty, databaseError: dbError)
            .Handle(command)
            .AssertDatabase(DatabaseState.Empty)
            .AssertOutput(dbError.AsTypedError<int>());
    }

    private static HandlerTestSetup<CreatePersonCommandHandler, int> Arrange(
        DatabaseState databaseState,
        Result? databaseError = null)
    {
        return new HandlerTestSetup<CreatePersonCommandHandler, int>(
            databaseState ?? DatabaseState.Empty,
            databaseError,
            configureMocker: mocker =>
            {
                mocker.Use<IPersonRepository>(new FakePersonRepository());
            });
    }
}
