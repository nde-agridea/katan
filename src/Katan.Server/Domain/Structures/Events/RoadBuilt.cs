namespace Katan.Server.Domain.Structures.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record RoadBuilt(string PlayerId, Edge Location) : DomainEvent;