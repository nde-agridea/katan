namespace Katan.Server.Domain.GameFlow.Events;

using Katan.Server.Domain;

public record TurnEnded(string PlayerId, string NextPlayerId) : DomainEvent;