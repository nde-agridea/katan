namespace Katan.Server.Domain.GameFlow;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.Structures;
using Katan.Server.Domain.Structures.Events;

public class InitialPlacementService
{
    private readonly Game _game;

    public InitialPlacementService(Game game)
    {
        _game = game;
    }

    public void BuildPlacementOrder()
    {
        var ids = _game.Players.Select(player => player.Id).ToList();
        var order = ids.Concat(ids.AsEnumerable().Reverse()).ToList();
        _game.SetPlacementOrder(order);
    }

    public string CurrentPlayerId => _game.PlacementOrder[_game.PlacementOrderIndex];

    public bool IsComplete => _game.PlacementOrderIndex >= _game.PlacementOrder.Count;

    public void PlaceSettlement(string playerId, Intersection location)
    {
        if (CurrentPlayerId != playerId)
            throw new InvalidOperationException("Not this player's turn to place.");

        var player = _game.Players.First(candidate => candidate.Id == playerId);
        var settlement = new Settlement(playerId, location);
        player.PlaceSettlement(settlement);
        _game.AddEvent(new InitialSettlementPlaced(playerId, location));

        var resources = new Dictionary<ResourceType, int>();
        foreach (var tilePosition in location.AdjacentTiles)
        {
            var tile = _game.Board.GetTile(tilePosition);
            if (tile?.Produces is { } resourceType)
            {
                resources[resourceType] = resources.GetValueOrDefault(resourceType, 0) + 1;
                player.Hand.Add(resourceType, 1);
            }
        }

        if (resources.Count > 0)
            _game.AddEvent(new StartingResourcesGranted(playerId, resources));
    }

    public void PlaceRoad(string playerId, Edge location)
    {
        if (CurrentPlayerId != playerId)
            throw new InvalidOperationException("Not this player's turn to place.");

        var player = _game.Players.First(candidate => candidate.Id == playerId);
        var road = new Road(playerId, location);
        player.PlaceRoad(road);
        _game.AddEvent(new InitialRoadPlaced(playerId, location));
    }

    public void AdvancePlacement() => _game.AdvancePlacementOrderIndex();
}