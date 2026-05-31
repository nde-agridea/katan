namespace Katan.Server.Domain.Board;

public static class EdgeExtensions
{
    /// <summary>
    /// Returns the (up to 2) intersections at the endpoints of this edge.
    /// For an edge between tiles A and B, the endpoints are the two common neighbours {A,B,C1} and {A,B,C2}.
    /// </summary>
    public static IEnumerable<Intersection> GetEndpoints(this Edge edge)
    {
        var tiles = edge.AdjacentTiles.ToList();
        if (tiles.Count < 2) yield break;

        var a = tiles[0];
        var b = tiles[1];
        foreach (var common in a.Neighbours().Intersect(b.Neighbours()))
            yield return new Intersection(new[] { a, b, common });
    }
}
