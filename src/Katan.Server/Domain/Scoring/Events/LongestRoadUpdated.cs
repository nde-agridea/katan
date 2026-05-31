namespace Katan.Server.Domain.Scoring.Events;

using Katan.Server.Domain;

public record LongestRoadUpdated(string? PlayerId, int Length) : DomainEvent;