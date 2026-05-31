namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;

public record SevenRolled(string ActivePlayerId) : DomainEvent;