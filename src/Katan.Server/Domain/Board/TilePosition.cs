namespace Katan.Server.Domain.Board;

public record TilePosition(int Q, int R)
{
    public IEnumerable<TilePosition> Neighbours()
    {
        yield return new TilePosition(Q + 1, R);
        yield return new TilePosition(Q - 1, R);
        yield return new TilePosition(Q, R + 1);
        yield return new TilePosition(Q, R - 1);
        yield return new TilePosition(Q + 1, R - 1);
        yield return new TilePosition(Q - 1, R + 1);
    }
}