namespace Katan.Server.Domain.Structures;

using Katan.Server.Domain.Board;

public class City
{
    public string PlayerId { get; }
    public Intersection Location { get; }

    public City(string playerId, Intersection location)
    {
        PlayerId = playerId;
        Location = location;
    }
}