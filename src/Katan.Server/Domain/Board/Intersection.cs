namespace Katan.Server.Domain.Board;

public sealed class Intersection : IEquatable<Intersection>
{
    private readonly IReadOnlySet<TilePosition> _adjacentTiles;

    public Intersection(IEnumerable<TilePosition> adjacentTiles)
    {
        _adjacentTiles = new HashSet<TilePosition>(adjacentTiles);
        if (_adjacentTiles.Count is < 1 or > 3)
            throw new ArgumentException("An intersection must be adjacent to 1–3 tiles.", nameof(adjacentTiles));
    }

    public IReadOnlySet<TilePosition> AdjacentTiles => _adjacentTiles;

    public bool Equals(Intersection? other) => other is not null && _adjacentTiles.SetEquals(other._adjacentTiles);

    public override bool Equals(object? obj) => obj is Intersection intersection && Equals(intersection);

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var position in _adjacentTiles)
            hash ^= position.GetHashCode();

        return hash;
    }
}