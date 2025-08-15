using Stargate.Core.Domain;

namespace Stargate.Testing;

public class AstronautDutyInfo
{
    public string? Rank { get; set; }
    public string? DutyTitle { get; set; }
    public DateTime? DutyStartDate { get; set; }
}

public static class DataModels
{
    public static Person CreatePerson(string? name = null)
    {
        return new Person(name ?? "James Bond");
    }

    public static Person CreateAstronaut(
        AstronautDutyInfo[] duties,
        string? name = null)
    {
        var person = CreatePerson();

        foreach (var dutyInfo in duties)
        {
            var duty = CreateAstronautDuty(person, dutyInfo.Rank, dutyInfo.DutyTitle, dutyInfo.DutyStartDate);
            person.AddAstronautDuty(duty.Rank, duty.DutyTitle, duty.DutyStartDate);
        }

        return person;
    }

    public static AstronautDuty CreateAstronautDuty(
        Person person,
        string? rank = null,
        string? dutyTitle = null,
        DateTime? dutyStartDate = null,
        DateTime? dutyEndDate = null)
    {
        return new AstronautDuty(
            person: person,
            rank: rank ?? "Rookie",
            dutyTitle: dutyTitle ?? "Junior Astronaut",
            dutyStartDate: dutyStartDate ?? DateTime.UtcNow,
            dutyEndDate: dutyEndDate);
    }
}
