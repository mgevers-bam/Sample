using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace Stargate.Core.Domain;

public class AstronautDuty : Entity<int>, IDataModel
{
    public const string Retired = "RETIRED";

    private AstronautDuty() { }

    public AstronautDuty(
        Person person,
        string rank,
        string dutyTitle,
        DateTime dutyStartDate)
    {
        Person = person;
        PersonId = person.Id;
        Rank = rank;
        DutyTitle = dutyTitle;
        DutyStartDate = dutyStartDate;
    }

    public AstronautDuty(
        Person person,
        string rank,
        string dutyTitle,
        DateTime dutyStartDate,
        DateTime? dutyEndDate)
    {
        Person = person;
        PersonId = person.Id;
        Rank = rank;
        DutyTitle = dutyTitle;
        DutyStartDate = dutyStartDate;
        DutyEndDate = dutyEndDate;
    }

    public int PersonId { get; private set; }

    public string Rank { get; private set; } = string.Empty;

    public string DutyTitle { get; private set; } = string.Empty;

    public DateTime DutyStartDate { get; private set; }

    public DateTime? DutyEndDate { get; set; }

    [JsonIgnore]
    public virtual Person Person { get; set; } = null!;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
