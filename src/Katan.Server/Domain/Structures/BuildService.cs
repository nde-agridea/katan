namespace Katan.Server.Domain.Structures;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Military;
using Katan.Server.Domain.Resources;
using Katan.Server.Domain.Structures.Events;

/// <summary>
/// Handles all building actions: road, settlement, city, army.
/// </summary>
public class BuildService
{
    private readonly Game _game;

    // Costs (US-BU1 through US-BU4)
    public static readonly IReadOnlyDictionary<string, Dictionary<ResourceType, int>> Costs =
        new Dictionary<string, Dictionary<ResourceType, int>>
        {
            ["road"] = new() { [ResourceType.Wood] = 1, [ResourceType.Brick] = 1 },
            ["settlement"] = new() { [ResourceType.Wood] = 2, [ResourceType.Brick] = 1, [ResourceType.Wheat] = 1, [ResourceType.Sheep] = 1 },
            ["city"] = new() { [ResourceType.Stone] = 3, [ResourceType.Wheat] = 2 },
            ["army"] = new() { [ResourceType.Iron] = 2, [ResourceType.Wheat] = 1 },
            ["devcard"] = new() { [ResourceType.Stone] = 1, [ResourceType.Sheep] = 1, [ResourceType.Iron] = 1 },
        };

    public BuildService(Game game)
    {
        _game = game;
    }

    // US-BU1: build a road
    public void BuildRoad(string playerId, Edge location, bool free = false)
    {
        var player = GetPlayer(playerId);

        if (player.RemainingRoads == 0)
            throw new InvalidOperationException("No roads remaining (piece limit: 15).");

        if (!IsRoadConnected(player, location))
            throw new InvalidOperationException("Road must be connected to the player's road/settlement network.");

        if (!free)
            Pay(player, Costs["road"]);

        var road = new Road(playerId, location);
        player.PlaceRoad(road);
        _game.AddEvent(new RoadBuilt(playerId, location));
    }

    // US-BU2: build a settlement (with road-connectivity; pass free=true and skipConnectivity=true for initial placement)
    public void BuildSettlement(string playerId, Intersection location, bool free = false, bool skipConnectivity = false)
    {
        var player = GetPlayer(playerId);

        if (player.RemainingSettlements == 0)
            throw new InvalidOperationException("No settlements remaining (piece limit: 5).");

        if (!skipConnectivity && !IsSettlementConnected(player, location))
            throw new InvalidOperationException("Settlement must be connected to the player's road network.");

        if (!free)
            Pay(player, Costs["settlement"]);

        var settlement = new Settlement(playerId, location);
        player.PlaceSettlement(settlement);
        _game.AddEvent(new SettlementBuilt(playerId, location));
    }

    // US-BU3: upgrade a settlement to a city
    public void UpgradeCity(string playerId, Intersection location)
    {
        var player = GetPlayer(playerId);

        if (player.RemainingCities == 0)
            throw new InvalidOperationException("No cities remaining (piece limit: 4).");

        var existing = player.Settlements.FirstOrDefault(s => s.Location.Equals(location))
            ?? throw new InvalidOperationException("No settlement at the specified location to upgrade.");

        Pay(player, Costs["city"]);

        var city = new City(playerId, location);
        player.PlaceCity(city, existing);
        _game.AddEvent(new CityUpgraded(playerId, location));
    }

    // US-BU4: build an army
    public void BuildArmy(string playerId, TilePosition tilePosition)
    {
        var player = GetPlayer(playerId);

        if (player.RemainingArmies == 0)
            throw new InvalidOperationException("No armies remaining (piece limit: 10).");

        bool hasStructureOnTile =
            player.Settlements.Any(s => s.Location.AdjacentTiles.Contains(tilePosition)) ||
            player.Cities.Any(c => c.Location.AdjacentTiles.Contains(tilePosition));

        if (!hasStructureOnTile)
            throw new InvalidOperationException("Army must be placed on a tile where the player has a settlement or city.");

        Pay(player, Costs["army"]);

        var army = new Army(playerId, tilePosition);
        player.PlaceArmy(army);

        // Claiming the tile if it's unclaimed
        var tile = _game.Board.GetTile(tilePosition);
        if (tile is not null && tile.OwnerId is null)
        {
            tile.Claim(playerId);
            _game.AddEvent(new Military.Events.TileClaimed(playerId, tilePosition));
        }

        _game.AddEvent(new Military.Events.ArmyBuilt(playerId, tilePosition));
    }

    // --- Connectivity helpers ---

    private bool IsRoadConnected(GameFlow.Player player, Edge edge)
    {
        var newEndpoints = edge.GetEndpoints().ToList();
        if (newEndpoints.Count == 0) return false;

        foreach (var endpoint in newEndpoints)
        {
            // Connected to a player settlement/city?
            if (player.Settlements.Any(s => s.Location.Equals(endpoint))) return true;
            if (player.Cities.Any(c => c.Location.Equals(endpoint))) return true;

            // Connected to an existing road endpoint?
            foreach (var road in player.Roads)
            {
                foreach (var roadEndpoint in road.Location.GetEndpoints())
                {
                    if (roadEndpoint.Equals(endpoint)) return true;
                }
            }
        }
        return false;
    }

    private bool IsSettlementConnected(GameFlow.Player player, Intersection location)
    {
        foreach (var road in player.Roads)
        {
            foreach (var endpoint in road.Location.GetEndpoints())
            {
                if (endpoint.Equals(location)) return true;
            }
        }
        return false;
    }

    private static void Pay(GameFlow.Player player, Dictionary<ResourceType, int> cost)
    {
        if (!player.Hand.CanAfford(cost))
            throw new InvalidOperationException("Insufficient resources.");
        player.Hand.Pay(cost);
    }

    private GameFlow.Player GetPlayer(string id) =>
        _game.Players.FirstOrDefault(p => p.Id == id)
        ?? throw new InvalidOperationException($"Player '{id}' not found.");
}
