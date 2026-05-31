namespace Katan.Server.Domain.Resources;

using Katan.Server.Domain.Board;

public class ResourceHand
{
    private readonly Dictionary<ResourceType, int> _cards = new();

    public ResourceHand()
    {
        foreach (var type in Enum.GetValues<ResourceType>())
            _cards[type] = 0;
    }

    public int Count(ResourceType type) => _cards.GetValueOrDefault(type, 0);

    public int Total => _cards.Values.Sum();

    public void Add(ResourceType type, int amount = 1)
    {
        if (amount < 0)
            throw new ArgumentException("Amount must be non-negative.");

        _cards[type] = _cards.GetValueOrDefault(type, 0) + amount;
    }

    public bool CanAfford(IReadOnlyDictionary<ResourceType, int> cost)
        => cost.All(kv => Count(kv.Key) >= kv.Value);

    public void Pay(IReadOnlyDictionary<ResourceType, int> cost)
    {
        if (!CanAfford(cost))
            throw new InvalidOperationException("Insufficient resources.");

        foreach (var (type, amount) in cost)
            _cards[type] -= amount;
    }

    public void Remove(ResourceType type, int amount = 1)
    {
        if (Count(type) < amount)
            throw new InvalidOperationException($"Insufficient {type}.");

        _cards[type] -= amount;
    }

    public IReadOnlyDictionary<ResourceType, int> GetAll() => _cards;
}