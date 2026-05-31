namespace Katan.Server.Domain.GameFlow.Events;

using Katan.Server.Domain;

public record TurnStarted(string ActivePlayerId) : DomainEvent;