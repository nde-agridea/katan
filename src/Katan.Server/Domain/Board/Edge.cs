namespace Katan.Server.Domain.Board;

public sealed class Edge : IEquatable<Edge>
{
    private readonly IReadOnlySet<TilePosition> _adjacentTiles;

    public Edge(TilePosition a, TilePosition b)
    {
        _adjacentTiles = new HashSet<TilePosition> { a, b };
    }

    public Edge(TilePosition coastal)
    {
        _adjacentTiles = new HashSet<TilePosition> { coastal };
    }

    public IReadOnlySet<TilePosition> AdjacentTiles => _adjacentTiles;

    public bool Equals(Edge? other) => other is not null && _adjacentTiles.SetEquals(other._adjacentTiles);

    public override bool Equals(object? obj) => obj is Edge edge && Equals(edge);

    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var position in _adjacentTiles)
            hash ^= position.GetHashCode();

        return hash;
    }
}