namespace Katan.Server.Domain.Resources;

using Katan.Server.Domain.Board;

public sealed record PendingTrade(
    string OfferingPlayerId,
    string TargetPlayerId,
    IReadOnlyDictionary<ResourceType, int> Offering,
    IReadOnlyDictionary<ResourceType, int> Requesting);
