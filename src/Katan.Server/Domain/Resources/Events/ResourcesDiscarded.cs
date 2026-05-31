namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record ResourcesDiscarded(string PlayerId, IReadOnlyDictionary<ResourceType, int> Discarded) : DomainEvent;