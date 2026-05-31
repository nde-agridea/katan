namespace Katan.Server.Domain.Structures.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record CityUpgraded(string PlayerId, Intersection Location) : DomainEvent;