namespace Katan.Server.Domain.Structures.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record StartingResourcesGranted(string PlayerId, IReadOnlyDictionary<ResourceType, int> Resources) : DomainEvent;