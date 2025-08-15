using Stargate.Testing;
using Stargate.Testing.Assert;

namespace Stargate.Core.Domain.Tests;

public class PersonTests
{
    [Fact]
    public void CanCreatePerson()
    {
        var name = "James Bond";
        var person = new Person(name);

        Assert.Equal(name, person.Name);
        Assert.Empty(person.AstronautDuties);
        Assert.Null(person.AstronautDetail);
    }

    [Fact]
    public void AddingSingleAstronautDuty_UpdatesAstronautDetail()
    {
        var person = new Person("James Bond");
        var duty = DataModels.CreateAstronautDuty(person);

        var addResult = person.AddAstronautDuty(duty.Rank, duty.DutyTitle, duty.DutyStartDate);

        Assert.True(addResult.IsSuccess);

        Assert.Equal(duty.Rank, person.AstronautDetail?.CurrentRank);
        Assert.Equal(duty.DutyTitle, person.AstronautDetail?.CurrentDutyTitle);
        Assert.Equal(duty.DutyStartDate, person.AstronautDetail?.CareerStartDate);
        Assert.Null(duty.DutyEndDate);
        Assert.Null(person.AstronautDetail?.CareerEndDate);

        AssertExtensions.DeepEqual([duty], person.AstronautDuties);
    }

    [Fact]
    public void AddingManyAstronautDuty_UpdatesAstronautDetail()
    {
        var person = new Person("James Bond");
        var duties = new List<AstronautDuty>
        {
            DataModels.CreateAstronautDuty(
                person,
                rank: "Lieutenant",
                dutyTitle: "Pilot",
                dutyStartDate: DateTime.Parse("1/1/2000"),
                dutyEndDate: DateTime.Parse("1/1/2010").AddDays(-1)),
            DataModels.CreateAstronautDuty(
                person,
                rank: "Captain",
                dutyTitle: "Commander",
                dutyStartDate: DateTime.Parse("1/1/2010")),
        };

        foreach (var duty in duties)
        {
            var addResult = person.AddAstronautDuty(duty.Rank, duty.DutyTitle, duty.DutyStartDate);
            Assert.True(addResult.IsSuccess);
        }

        Assert.Equal(duties.Last().Rank, person.AstronautDetail?.CurrentRank);
        Assert.Equal(duties.Last().DutyTitle, person.AstronautDetail?.CurrentDutyTitle);
        Assert.Equal(duties.First().DutyStartDate, person.AstronautDetail?.CareerStartDate);
        Assert.Null(duties.Last().DutyEndDate);
        Assert.Null(person.AstronautDetail?.CareerEndDate);

        AssertExtensions.DeepEqual(duties, person.AstronautDuties);
    }

    [Fact]
    public void AddingRetiredAstronautDuty_UpdatesAstronautDetail()
    {
        var person = new Person("James Bond");
        var duties = new List<AstronautDuty>
        {
            DataModels.CreateAstronautDuty(
                person,
                rank: "Lieutenant",
                dutyTitle: "Pilot",
                dutyStartDate: DateTime.Parse("1/1/2000"),
                dutyEndDate: DateTime.Parse("1/1/2010").AddDays(-1)),
            DataModels.CreateAstronautDuty(
                person,
                rank: "Captain",
                dutyTitle: "Commander",
                dutyStartDate: DateTime.Parse("1/1/2010"),
                dutyEndDate: DateTime.Parse("1/1/2020").AddDays(-1)),
            DataModels.CreateAstronautDuty(
                person,
                rank: "General",
                dutyTitle: AstronautDuty.Retired,
                dutyStartDate: DateTime.Parse("1/1/2020"))
        };

        foreach (var duty in duties)
        {
            var addResult = person.AddAstronautDuty(duty.Rank, duty.DutyTitle, duty.DutyStartDate);
            Assert.True(addResult.IsSuccess);
        }

        Assert.Equal(duties.Last().Rank, person.AstronautDetail?.CurrentRank);
        Assert.Equal(duties.Last().DutyTitle, person.AstronautDetail?.CurrentDutyTitle);
        Assert.Equal(duties.First().DutyStartDate, person.AstronautDetail?.CareerStartDate);
        Assert.Equal(DateTime.Parse("1/1/2020").AddDays(-1), person.AstronautDetail?.CareerEndDate);

        AssertExtensions.DeepEqual(duties, person.AstronautDuties);
    }
    [Fact]
    public void AddingAstronautDuty_WhenAlreadyRetired_ReturnsFailure()
    {
        var person = new Person("James Bond");
        var duties = new List<AstronautDuty>
        {
            DataModels.CreateAstronautDuty(
                person,
                rank: "Lieutenant",
                dutyTitle: "Pilot",
                dutyStartDate: DateTime.Parse("1/1/2000"),
                dutyEndDate: DateTime.Parse("1/1/2010").AddDays(-1)),
            DataModels.CreateAstronautDuty(
                person,
                rank: "Captain",
                dutyTitle: "Commander",
                dutyStartDate: DateTime.Parse("1/1/2010"),
                dutyEndDate: DateTime.Parse("1/1/2020").AddDays(-1)),
            DataModels.CreateAstronautDuty(
                person,
                rank: "General",
                dutyTitle: AstronautDuty.Retired,
                dutyStartDate: DateTime.Parse("1/1/2020"))
        };

        foreach (var duty in duties)
        {
            var addResult = person.AddAstronautDuty(duty.Rank, duty.DutyTitle, duty.DutyStartDate);
            Assert.True(addResult.IsSuccess);
        }

        var failingDuty = DataModels.CreateAstronautDuty(person);
        var failingAddResult = person.AddAstronautDuty(failingDuty.Rank, failingDuty.DutyTitle, failingDuty.DutyStartDate);
        Assert.False(failingAddResult.IsSuccess);
    }

}
