using Ardalis.Result;
using Common.LanguageExtensions.Utilities.ResultExtensions;
using Common.Testing.FluentTesting;
using Common.Testing.FluentTesting.Asserts;
using Common.Testing.Persistence;
using Stargate.Core.Commands;
using Stargate.Core.Contracts;
using Stargate.Core.Domain;
using Stargate.Core.Tests.Fakes;
using Stargate.Testing;

namespace Stargate.Core.Tests.Commands;

public class CreateAstronautDutyTests
{
    [Fact]
    public async Task CanAddAstronautDuty()
    {
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);
        var astronaut = DataModels.CreateAstronaut([duty], person.Name);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };

        await Arrange(new DatabaseState(person))
            .Handle(command)
            .AssertDatabase(new DatabaseState(astronaut))
            .AssertOutput(Result.Success(0));
    }

    [Fact]
    public async Task AddAstronautDuty_WhenPersonNotFound_ReturnsFailure()
    {
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };

        await Arrange(DatabaseState.Empty)
            .Handle(command)
            .AssertDatabase(DatabaseState.Empty)
            .AssertOutput(Result.NotFound().AsTypedError<int>());
    }

    [Fact]
    public async Task AddAstronautDuty_WhenPersonAlreadyRetired_ReturnsFailure()
    {
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person, dutyTitle: AstronautDuty.Retired);
        var astronaut = DataModels.CreateAstronaut([duty], person.Name);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };

        await Arrange(new DatabaseState(astronaut))
            .Handle(command)
            .AssertDatabase(new DatabaseState(astronaut))
            .AssertOutput(Result.Error("Cannot add a new duty after retirement.").AsTypedError<int>());
    }

    [Fact]
    public async Task AddAstronautDuty_WhenSaveFails_ReturnsFailure()
    {
        var dbError = Result.Conflict("A Person with the name already exists");
        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);

        var command = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };

        await Arrange(new DatabaseState(person), databaseError: dbError)
            .Handle(command)
            .AssertDatabase(new DatabaseState(person))
            .AssertOutput(dbError.AsTypedError<int>());
    }

    private static HandlerTestSetup<CreateAstronautDutyCommandHandler, int> Arrange(
        DatabaseState databaseState,
        Result? databaseError = null)
    {
        return new HandlerTestSetup<CreateAstronautDutyCommandHandler, int>(
            databaseState ?? DatabaseState.Empty,
            databaseError,
            configureMocker: mocker =>
            {
                mocker.Use<IPersonRepository>(new FakePersonRepository());
            });
    }
}
