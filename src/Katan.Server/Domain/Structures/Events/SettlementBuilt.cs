namespace Katan.Server.Domain.Structures.Events;

using Katan.Server.Domain;
using Katan.Server.Domain.Board;

public record SettlementBuilt(string PlayerId, Intersection Location) : DomainEvent;