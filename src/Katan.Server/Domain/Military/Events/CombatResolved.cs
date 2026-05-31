namespace Katan.Server.Domain.Military.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record CombatResolved(string AttackerId, string DefenderId, TilePosition TilePosition, bool AttackerWon, int AttackerRoll, int DefenderRoll) : DomainEvent;