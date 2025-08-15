using CSharpFunctionalExtensions;
using Newtonsoft.Json;

namespace Stargate.Core.Domain;

public class AstronautDetail : Entity<int>, IDataModel
{
    private AstronautDetail() { }

    public AstronautDetail(
        Person person,
        string currentRank,
        string currentDutyTitle,
        DateTime careerStartDate)
    {
        Person = person;
        PersonId = person.Id;
        CurrentRank = currentRank;
        CurrentDutyTitle = currentDutyTitle;
        CareerStartDate = careerStartDate;
    }

    public int PersonId { get; private set; }

    public string CurrentRank { get; set; } = string.Empty;

    public string CurrentDutyTitle { get; set; } = string.Empty;

    public DateTime CareerStartDate { get; private set; }

    public DateTime? CareerEndDate { get; set; }

    public virtual Person Person { get; private set; } = null!;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
