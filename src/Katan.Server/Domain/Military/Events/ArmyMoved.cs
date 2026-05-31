namespace Katan.Server.Domain.Military.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record ArmyMoved(string PlayerId, TilePosition From, TilePosition To) : DomainEvent;