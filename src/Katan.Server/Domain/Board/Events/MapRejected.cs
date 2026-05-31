namespace Katan.Server.Domain.Board.Events;

using Katan.Server.Domain;

public record MapRejected(int GenerationAttempt) : DomainEvent;