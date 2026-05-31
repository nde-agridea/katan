namespace Katan.Server.Domain.Military.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record TileConquered(string AttackerId, string PreviousOwnerId, TilePosition Position) : DomainEvent;