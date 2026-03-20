using MediatR;

namespace Stargate.Core.Boundary.Events;

public class AstronautDutyCreatedEvent : INotification
{
    public required int PersonId { get; set; }

    public required string Name { get; set; }

    public required string Rank { get; set; }

    public required string DutyTitle { get; set; }

    public DateTime DutyStartDate { get; set; }
}
