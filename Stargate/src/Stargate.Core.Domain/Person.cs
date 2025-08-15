using Ardalis.Result;
using Newtonsoft.Json;
using C = CSharpFunctionalExtensions;

namespace Stargate.Core.Domain;

public class Person : C.Entity<int>, IDataModel
{
    private Person() { }

    public Person(string name)
    {
        Name = name;
    }

    public string Name { get; private set; } = string.Empty;

    public virtual AstronautDetail? AstronautDetail { get; private set; }

    public virtual IReadOnlyList<AstronautDuty> AstronautDuties { get; private set; } = new List<AstronautDuty>();

    public Result AddAstronautDuty(string rank, string dutyTitle, DateTime dutyStartDate)
    {
        var lastDuty = AstronautDuties.LastOrDefault();

        if (lastDuty != null)
        {
            if (lastDuty.DutyTitle == AstronautDuty.Retired)
            {
                return Result.Error("Cannot add a new duty after retirement.");
            }

            lastDuty.DutyEndDate = dutyStartDate.AddDays(-1);
        }

        var newDuty = new AstronautDuty(
            person: this,
            rank,
            dutyTitle,
            dutyStartDate);

        AstronautDuties = [.. AstronautDuties, newDuty];

        if (AstronautDetail == null)
        {
            AstronautDetail = new AstronautDetail(
                person: this,
                rank,
                dutyTitle,
                dutyStartDate);
        }
        else
        {
            AstronautDetail.CurrentRank = rank;
            AstronautDetail.CurrentDutyTitle = dutyTitle;
            if (dutyTitle == AstronautDuty.Retired)
            {
                AstronautDetail.CareerEndDate = dutyStartDate.AddDays(-1).Date;
            }
        }

        return Result.Success();
    }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}
