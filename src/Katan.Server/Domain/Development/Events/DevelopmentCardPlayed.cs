namespace Katan.Server.Domain.Development.Events;

using Katan.Server.Domain;

public record DevelopmentCardPlayed(string PlayerId, DevelopmentCardType CardType) : DomainEvent;