namespace Katan.Server.Domain.Board;

using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Resources.Events;

public class RobberService
{
    private readonly Game _game;
    private readonly Random _random;

    public RobberService(Game game, Random? random = null)
    {
        _game = game;
        _random = random ?? Random.Shared;
    }

    public void StealFromOpponent(string movingPlayerId, string targetPlayerId)
    {
        var target = _game.Players.FirstOrDefault(player => player.Id == targetPlayerId)
            ?? throw new InvalidOperationException("Target player not found.");
        var mover = _game.Players.FirstOrDefault(player => player.Id == movingPlayerId)
            ?? throw new InvalidOperationException("Moving player not found.");

        var robberPosition = _game.Board.RobberPosition
            ?? throw new InvalidOperationException("Robber not placed.");

        var hasStructure = target.Settlements.Any(settlement => settlement.Location.AdjacentTiles.Contains(robberPosition))
            || target.Cities.Any(city => city.Location.AdjacentTiles.Contains(robberPosition));
        if (!hasStructure)
            throw new InvalidOperationException("Target has no structure on robber tile.");

        var available = target.Hand.GetAll().Where(kv => kv.Value > 0).ToList();
        if (available.Count == 0)
            return;

        var chosen = available[_random.Next(available.Count)].Key;
        target.Hand.Remove(chosen);
        mover.Hand.Add(chosen);
        _game.AddEvent(new RobberTributeCollected(targetPlayerId, chosen));
    }

    public void PayTribute(string playerId, ResourceType resource)
    {
        var player = _game.Players.FirstOrDefault(candidate => candidate.Id == playerId)
            ?? throw new InvalidOperationException("Player not found.");

        var robberPosition = _game.Board.RobberPosition
            ?? throw new InvalidOperationException("Robber not placed.");

        var hasStructure = player.Settlements.Any(settlement => settlement.Location.AdjacentTiles.Contains(robberPosition))
            || player.Cities.Any(city => city.Location.AdjacentTiles.Contains(robberPosition));
        if (!hasStructure)
            throw new InvalidOperationException("Player has no structure on robber tile.");

        player.Hand.Remove(resource);
        _game.AddEvent(new RobberTributeCollected(playerId, resource));
    }
}