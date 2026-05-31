namespace Katan.Server.Domain.Development;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Development.Events;
using Katan.Server.Domain.Resources.Events;
using Katan.Server.Domain.Structures;

public class DevelopmentCardService
{
    private readonly Game _game;
    private readonly DevelopmentCardDeck _deck;
    private readonly Structures.BuildService _buildService;

    public DevelopmentCardService(Game game, DevelopmentCardDeck? deck = null, Structures.BuildService? buildService = null)
    {
        _game = game;
        _deck = deck ?? new DevelopmentCardDeck();
        _buildService = buildService ?? new Structures.BuildService(game);
    }

    // US-D1: purchase and immediately play a development card
    public DevelopmentCard PurchaseAndPlay(string playerId)
    {
        var player = GetPlayer(playerId);
        var cost = Structures.BuildService.Costs["devcard"];

        if (!player.Hand.CanAfford(cost))
            throw new InvalidOperationException("Insufficient resources to purchase a development card.");

        player.Hand.Pay(cost);
        var card = _deck.Draw();
        _game.AddEvent(new DevelopmentCardPurchased(playerId, card.Type));
        return card;
    }

    // US-D2: Knight card – move robber and optionally steal
    public void PlayKnight(string playerId, TilePosition newRobberPosition, string? stealFromPlayerId = null)
    {
        _game.Board.PlaceRobber(newRobberPosition);
        _game.AddEvent(new KnightPlayed(playerId, newRobberPosition));
        _game.AddEvent(new DevelopmentCardPlayed(playerId, DevelopmentCardType.Knight));

        if (stealFromPlayerId is not null)
        {
            var robberService = new Domain.Board.RobberService(_game);
            robberService.StealFromOpponent(playerId, stealFromPlayerId);
        }
    }

    // US-D3: Victory Point card – +1 VP immediately
    public void PlayVictoryPoint(string playerId)
    {
        var player = GetPlayer(playerId);
        player.VictoryPoints.AddDevelopmentCardPoint();
        _game.AddEvent(new DevelopmentCardPlayed(playerId, DevelopmentCardType.VictoryPoint));
    }

    // US-D4: Road Building card – 1 free road placed immediately
    public void PlayRoadBuilding(string playerId, Edge roadLocation)
    {
        _buildService.BuildRoad(playerId, roadLocation, free: true);
        _game.AddEvent(new RoadBuildingPlayed(playerId, roadLocation));
        _game.AddEvent(new DevelopmentCardPlayed(playerId, DevelopmentCardType.RoadBuilding));
    }

    // US-D5: Monopoly card – all opponents give 1 card of named resource
    public void PlayMonopoly(string playerId, ResourceType resourceType)
    {
        var player = GetPlayer(playerId);

        foreach (var other in _game.Players)
        {
            if (other.Id == playerId) continue;
            if (other.Hand.Count(resourceType) > 0)
            {
                other.Hand.Remove(resourceType);
                player.Hand.Add(resourceType);
            }
        }

        _game.AddEvent(new MonopolyPlayed(playerId, resourceType));
        _game.AddEvent(new DevelopmentCardPlayed(playerId, DevelopmentCardType.Monopoly));
    }

    // US-D6: Excess card – take 2 resources of choice from the bank
    public void PlayExcess(string playerId, ResourceType resource1, ResourceType resource2)
    {
        var player = GetPlayer(playerId);
        player.Hand.Add(resource1);
        player.Hand.Add(resource2);
        _game.AddEvent(new ExcessPlayed(playerId, resource1, resource2));
        _game.AddEvent(new DevelopmentCardPlayed(playerId, DevelopmentCardType.Excess));
    }

    // US-D7: Disaster card – destroy settlement/city on chosen tile, halve armies
    public void PlayDisaster(string playerId, TilePosition affectedTile)
    {
        // Destroy one settlement or city on the tile (any player's)
        foreach (var victim in _game.Players)
        {
            var settlement = victim.Settlements.FirstOrDefault(s => s.Location.AdjacentTiles.Contains(affectedTile));
            if (settlement is not null)
            {
                victim.RemoveSettlement(settlement);
                break;
            }
            var city = victim.Cities.FirstOrDefault(c => c.Location.AdjacentTiles.Contains(affectedTile));
            if (city is not null)
            {
                victim.RemoveCity(city);
                break;
            }
        }

        // Halve all armies on the tile (rounded down)
        foreach (var victim in _game.Players)
        {
            var armies = victim.Armies.Where(a => a.Position == affectedTile).ToList();
            int toRemove = armies.Count / 2;
            for (int i = 0; i < toRemove; i++)
                victim.ReturnArmyToSupply(armies[i]);
        }

        _game.AddEvent(new DisasterPlayed(playerId, affectedTile));
        _game.AddEvent(new DevelopmentCardPlayed(playerId, DevelopmentCardType.Disaster));
    }

    private GameFlow.Player GetPlayer(string id) =>
        _game.Players.FirstOrDefault(p => p.Id == id)
        ?? throw new InvalidOperationException($"Player '{id}' not found.");
}
