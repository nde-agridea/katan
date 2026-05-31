namespace Katan.Server.Domain.Military.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record ArmyBuilt(string PlayerId, TilePosition Position) : DomainEvent;