namespace Katan.Server.Domain.GameFlow.Events;

using Katan.Server.Domain;

public record GameStarted(string FirstPlayerId) : DomainEvent;