namespace Katan.Server.Domain.Development.Events;

using Katan.Server.Domain;

public record DevelopmentCardPurchased(string PlayerId, DevelopmentCardType CardType) : DomainEvent;