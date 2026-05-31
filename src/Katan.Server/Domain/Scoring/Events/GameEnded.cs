namespace Katan.Server.Domain.Scoring.Events;

using Katan.Server.Domain;

public record GameEnded(string WinnerId, int FinalVP) : DomainEvent;