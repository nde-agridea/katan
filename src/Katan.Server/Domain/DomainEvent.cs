namespace Katan.Server.Domain;

public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}