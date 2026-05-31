namespace Katan.Server.Domain.Development.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record RoadBuildingPlayed(string PlayerId, Edge Location) : DomainEvent;