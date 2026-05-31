namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;

public record TradeDeclined(string OfferingPlayerId, string DecliningPlayerId) : DomainEvent;