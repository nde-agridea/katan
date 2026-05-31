namespace Katan.Server.Domain.Structures.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record InitialSettlementPlaced(string PlayerId, Intersection Location) : DomainEvent;