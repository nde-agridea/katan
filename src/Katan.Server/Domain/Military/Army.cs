namespace Katan.Server.Domain.Military;

using Katan.Server.Domain.Board;

public class Army
{
    public string PlayerId { get; }
    public TilePosition Position { get; private set; }

    public Army(string playerId, TilePosition position)
    {
        PlayerId = playerId;
        Position = position;
    }

    public void MoveTo(TilePosition newPosition) => Position = newPosition;
}