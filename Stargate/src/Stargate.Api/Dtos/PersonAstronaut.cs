using Stargate.Core.Domain;

namespace Stargate.Api.Dtos;

public class PersonAstronaut
{
    public PersonAstronaut() { }

    public PersonAstronaut(Person person)
    {
        PersonId = person.Id;
        Name = person.Name;
        CurrentRank = person.AstronautDetail?.CurrentRank;
        CurrentDutyTitle = person.AstronautDetail?.CurrentDutyTitle;
        CareerStartDate = person.AstronautDetail?.CareerStartDate;
        CareerEndDate = person.AstronautDetail?.CareerEndDate;
    }

    public int PersonId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? CurrentRank { get; set; }

    public string? CurrentDutyTitle { get; set; }

    public DateTime? CareerStartDate { get; set; }

    public DateTime? CareerEndDate { get; set; }
}
