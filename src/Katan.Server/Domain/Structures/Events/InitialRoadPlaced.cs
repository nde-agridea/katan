namespace Katan.Server.Domain.Structures.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record InitialRoadPlaced(string PlayerId, Edge Location) : DomainEvent;