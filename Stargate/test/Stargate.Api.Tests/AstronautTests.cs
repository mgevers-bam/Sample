using Common.Testing.Web.Auth;
using Stargate.Core.Commands;
using Stargate.Testing;
using System.Security.Claims;

namespace Stargate.Api.Tests;

public class AstronautTests : IClassFixture<StargateApiApplicationFactory>
{
    private readonly StargateApiApplicationFactory _factory;

    public AstronautTests(StargateApiApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AstronautCrudTest()
    {
        var token = FakeJwtTokens.GenerateJwtToken(new Claim("PersonIdentifier", Guid.NewGuid().ToString()));
        //await _factory.InitializeDatabaseAsync();
        
        var client = _factory.CreateClient();

        var person = DataModels.CreatePerson();
        var duty = DataModels.CreateAstronautDuty(person);

        var createPersonCommand = new CreatePersonCommand
        {
            Name = person.Name,
        };
        var createPersonResponse = await client.PostAsync("/person", createPersonCommand);
        Assert.True(createPersonResponse.IsSuccessStatusCode);

        var getPersonResponse = await client.GetAsync($"/person/{person.Name}");
        Assert.True(getPersonResponse.IsSuccessStatusCode);

        var createDutyCommand = new CreateAstronautDutyCommand
        {
            Name = person.Name,
            Rank = duty.Rank,
            DutyTitle = duty.DutyTitle,
            DutyStartDate = duty.DutyStartDate
        };
        var createDutyResponse = await client.PostAsync("/astronautDuty", createDutyCommand);
        Assert.True(createDutyResponse.IsSuccessStatusCode);

        var getDutyResponse = await client.GetAsync($"/astronautDuty/{person.Name}");
        Assert.True(getDutyResponse.IsSuccessStatusCode);
    }
}
