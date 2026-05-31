namespace Katan.Server.Domain.Board;

public record ResourceCost(IReadOnlyDictionary<ResourceType, int> Amounts)
{
    public static ResourceCost Empty => new(new Dictionary<ResourceType, int>());

    public static ResourceCost Of(params (ResourceType type, int count)[] items)
        => new(items.ToDictionary(x => x.type, x => x.count));

    public bool IsEmpty => Amounts.Values.All(value => value == 0);
}