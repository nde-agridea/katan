namespace Katan.Server.Domain.Structures;

using Katan.Server.Domain.Board;

public class Road
{
    public string PlayerId { get; }
    public Edge Location { get; }

    public Road(string playerId, Edge location)
    {
        PlayerId = playerId;
        Location = location;
    }
}