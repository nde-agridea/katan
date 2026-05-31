namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;

public record TradeAccepted(string OfferingPlayerId, string AcceptingPlayerId) : DomainEvent;