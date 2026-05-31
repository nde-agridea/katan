namespace Katan.Server.Domain.Scoring.Events;

using Katan.Server.Domain;

public record VictoryPointsUpdated(string PlayerId, int Total) : DomainEvent;