namespace Katan.Server.Domain.Board;

public class Board
{
    private readonly Dictionary<TilePosition, Tile> _tiles = new();
    private readonly List<Port> _ports = new();

    public TilePosition? RobberPosition { get; private set; }
    public IReadOnlyDictionary<TilePosition, Tile> Tiles => _tiles;
    public IReadOnlyList<Port> Ports => _ports;

    public void AddTile(Tile tile) => _tiles[tile.Position] = tile;

    public Tile? GetTile(TilePosition position) => _tiles.GetValueOrDefault(position);

    public void AddPort(Port port) => _ports.Add(port);

    public void PlaceRobber(TilePosition position)
    {
        if (!_tiles.ContainsKey(position))
            throw new InvalidOperationException("Cannot place robber on non-existent tile.");

        RobberPosition = position;
    }

    public IEnumerable<Tile> GetNeighbours(TilePosition position)
        => position.Neighbours().Where(_tiles.ContainsKey).Select(p => _tiles[p]);

    public IEnumerable<Tile> GetTilesWithToken(int token)
        => _tiles.Values.Where(tile => tile.NumberToken == token);

    public bool IsCoastalTile(TilePosition position)
        => _tiles.ContainsKey(position) && position.Neighbours().Any(neighbour => !_tiles.ContainsKey(neighbour));
}