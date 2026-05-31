namespace Katan.Server.Domain.Board.Events;

using Katan.Server.Domain;

public record RobberMoved(string PlayerId, TilePosition NewPosition) : DomainEvent;