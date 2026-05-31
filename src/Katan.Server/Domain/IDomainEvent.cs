namespace Katan.Server.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}