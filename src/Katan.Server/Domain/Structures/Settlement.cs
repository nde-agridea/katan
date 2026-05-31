namespace Katan.Server.Domain.Structures;

using Katan.Server.Domain.Board;

public class Settlement
{
    public string PlayerId { get; }
    public Intersection Location { get; }

    public Settlement(string playerId, Intersection location)
    {
        PlayerId = playerId;
        Location = location;
    }
}