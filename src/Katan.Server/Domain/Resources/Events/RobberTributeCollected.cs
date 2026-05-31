namespace Katan.Server.Domain.Resources.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record RobberTributeCollected(string PlayerId, ResourceType Resource) : DomainEvent;