namespace Katan.Server.Domain.Development.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record KnightPlayed(string PlayerId, TilePosition NewRobberPosition) : DomainEvent;