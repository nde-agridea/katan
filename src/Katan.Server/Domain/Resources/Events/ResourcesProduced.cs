namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record ResourcesProduced(string PlayerId, ResourceType Type, int Amount) : DomainEvent;