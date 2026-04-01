using CSharpFunctionalExtensions;

namespace Common.LanguageExtensions.Contracts;

public abstract class EventSourcedEntity<TDomainEvent, TEventData> : Entity<Guid>
    where TDomainEvent : DomainEvent<TEventData>
    where TEventData : IEventData
{
    protected EventSourcedEntity(Guid id) : base(id) { }

    protected EventSourcedEntity() : base() { }

    public List<TDomainEvent> DomainEvents { get; protected set; } = [];

    protected void AddDomainEvent(TEventData eventData)
    {
        DomainEvents = [.. DomainEvents, CreateDomainEvent(sequenceId: DomainEvents.Count + 1, eventData)];
    }

    protected abstract TDomainEvent CreateDomainEvent(int sequenceId, TEventData eventData);
}
