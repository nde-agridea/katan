namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record BankTradeExecuted(string PlayerId, ResourceType Gave, int Amount, ResourceType Received) : DomainEvent;