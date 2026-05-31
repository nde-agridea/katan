namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record TradeOffered(string OfferingPlayerId, string TargetPlayerId, IReadOnlyDictionary<ResourceType, int> Offering, IReadOnlyDictionary<ResourceType, int> Requesting) : DomainEvent;