namespace Katan.Server.Domain.Scoring.Events;

using Katan.Server.Domain;

public record LargestArmyUpdated(string? PlayerId, int Count) : DomainEvent;