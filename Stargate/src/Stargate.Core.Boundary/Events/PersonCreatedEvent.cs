using MediatR;

namespace Stargate.Core.Boundary.Events;

public class PersonCreatedEvent : INotification
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
