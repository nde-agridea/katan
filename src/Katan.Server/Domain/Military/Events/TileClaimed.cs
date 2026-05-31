namespace Katan.Server.Domain.Military.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record TileClaimed(string PlayerId, TilePosition Position) : DomainEvent;