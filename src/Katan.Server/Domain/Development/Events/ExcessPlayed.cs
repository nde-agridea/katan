namespace Katan.Server.Domain.Development.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record ExcessPlayed(string PlayerId, ResourceType Resource1, ResourceType Resource2) : DomainEvent;