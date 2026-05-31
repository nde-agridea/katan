namespace Katan.Server.Domain.GameFlow.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record DiceRolled(string PlayerId, DiceResult Result) : DomainEvent;