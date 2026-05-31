namespace Katan.Server.Domain.Board.Events;

using Katan.Server.Domain;

public record MapVotecast(string PlayerId, bool Approve) : DomainEvent;