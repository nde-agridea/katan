namespace Katan.Server.Domain.Board;

public class Tile
{
    private readonly List<string> _armies = new();

    public TilePosition Position { get; }
    public TileType Type { get; }
    public int NumberToken { get; private set; }
    public string? OwnerId { get; private set; }
    public IReadOnlyList<string> Armies => _armies;

    public ResourceType? Produces => Type switch
    {
        TileType.Forest => ResourceType.Wood,
        TileType.Quarry => ResourceType.Brick,
        TileType.Field => ResourceType.Wheat,
        TileType.Pasture => ResourceType.Sheep,
        TileType.Mountain => ResourceType.Stone,
        TileType.Mine => ResourceType.Iron,
        _ => null
    };

    public Tile(TilePosition position, TileType type, int numberToken = 0)
    {
        Position = position;
        Type = type;
        NumberToken = numberToken;
    }

    public void SetNumberToken(int token) => NumberToken = token;

    public void Claim(string playerId) => OwnerId = playerId;

    public void TransferOwnership(string newOwnerId) => OwnerId = newOwnerId;

    public void AddArmy(string playerId) => _armies.Add(playerId);

    public void RemoveArmy(string playerId) => _armies.Remove(playerId);
}