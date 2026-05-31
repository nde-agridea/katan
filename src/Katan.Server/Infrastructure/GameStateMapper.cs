namespace Katan.Server.Infrastructure;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Military;
using Katan.Server.Domain.Structures;
using Katan.Shared.Protos;

/// <summary>Translates Game aggregate state into a GameState proto snapshot.</summary>
public static class GameStateMapper
{
    public static GameState Map(Game game)
    {
        var state = new GameState
        {
            GameId = game.Id.ToString(),
            GamePhase = MapGamePhase(game.Phase),
            TurnPhase = MapTurnPhase(game.TurnPhase),
            ActivePlayerId = game.ActivePlayer?.Id ?? string.Empty,
            WinnerId = string.Empty,
        };

        state.Board = MapBoard(game.Board);

        foreach (var player in game.Players)
        {
            state.Players.Add(MapPlayer(player));

            foreach (var s in player.Settlements)
                state.Settlements.Add(MapSettlement(s));
            foreach (var c in player.Cities)
                state.Cities.Add(MapCity(c));
            foreach (var r in player.Roads)
                state.Roads.Add(MapRoad(r));
            foreach (var a in player.Armies)
                state.Armies.Add(MapArmy(a));
        }

        // Set winner from GameEnded event if present
        var gameEnded = game.DomainEvents
            .OfType<Domain.Scoring.Events.GameEnded>()
            .LastOrDefault();
        if (gameEnded is not null)
            state.WinnerId = gameEnded.WinnerId;

        return state;
    }

    private static BoardStateProto MapBoard(Domain.Board.Board board)
    {
        var proto = new BoardStateProto();

        foreach (var tile in board.Tiles.Values)
        {
            proto.Tiles.Add(new TileProto
            {
                Position = MapPosition(tile.Position),
                Type = MapTileType(tile.Type),
                Token = tile.NumberToken,
                OwnerId = tile.OwnerId ?? string.Empty,
            });
        }

        foreach (var port in board.Ports)
        {
            var p = new PortProto
            {
                CoastalEdge = MapEdge(port.CoastalEdge),
                PortType = port.PortType == PortType.Generic ? PortTypeProto.Generic : PortTypeProto.Specialized,
                Resource = port.ResourceType.HasValue ? MapResourceType(port.ResourceType.Value) : ResourceTypeProto.ResourceTypeUnspecified,
            };
            proto.Ports.Add(p);
        }

        if (board.RobberPosition is not null)
            proto.RobberPosition = MapPosition(board.RobberPosition);

        return proto;
    }

    private static PlayerStateProto MapPlayer(Domain.GameFlow.Player player)
    {
        var proto = new PlayerStateProto
        {
            Id = player.Id,
            Name = player.Name,
            RemainingSettlements = player.RemainingSettlements,
            RemainingCities = player.RemainingCities,
            RemainingRoads = player.RemainingRoads,
            RemainingArmies = player.RemainingArmies,
            VictoryPoints = player.VictoryPoints.Total,
            HasLongestRoad = player.VictoryPoints.HasLongestRoad,
            HasLargestArmy = player.VictoryPoints.HasLargestArmy,
        };

        foreach (var (type, amount) in player.Hand.GetAll())
        {
            if (amount > 0)
                proto.Hand.Add(new ResourceAmountProto { Type = MapResourceType(type), Amount = amount });
        }

        return proto;
    }

    private static StructureProto MapSettlement(Settlement s) => new()
    {
        PlayerId = s.PlayerId,
        Location = MapIntersection(s.Location),
    };

    private static StructureProto MapCity(City c) => new()
    {
        PlayerId = c.PlayerId,
        Location = MapIntersection(c.Location),
    };

    private static RoadProto MapRoad(Road r) => new()
    {
        PlayerId = r.PlayerId,
        Location = MapEdge(r.Location),
    };

    private static ArmyProto MapArmy(Army a) => new()
    {
        PlayerId = a.PlayerId,
        Position = MapPosition(a.Position),
    };

    private static TilePositionProto MapPosition(TilePosition p) => new() { Q = p.Q, R = p.R };

    private static IntersectionProto MapIntersection(Intersection i)
    {
        var proto = new IntersectionProto();
        foreach (var t in i.AdjacentTiles)
            proto.AdjacentTiles.Add(MapPosition(t));
        return proto;
    }

    private static EdgeProto MapEdge(Edge e)
    {
        var proto = new EdgeProto();
        foreach (var t in e.AdjacentTiles)
            proto.AdjacentTiles.Add(MapPosition(t));
        return proto;
    }

    // Reverse mappings (proto → domain)

    public static TilePosition ToTilePosition(TilePositionProto proto) => new(proto.Q, proto.R);

    public static Intersection ToIntersection(IntersectionProto proto) =>
        new(proto.AdjacentTiles.Select(t => new TilePosition(t.Q, t.R)));

    public static Edge ToEdge(EdgeProto proto)
    {
        var tiles = proto.AdjacentTiles.Select(t => new TilePosition(t.Q, t.R)).ToList();
        return tiles.Count == 1 ? new Edge(tiles[0]) : new Edge(tiles[0], tiles[1]);
    }

    public static ResourceType ToResourceType(ResourceTypeProto proto) => proto switch
    {
        ResourceTypeProto.Wood  => ResourceType.Wood,
        ResourceTypeProto.Brick => ResourceType.Brick,
        ResourceTypeProto.Wheat => ResourceType.Wheat,
        ResourceTypeProto.Sheep => ResourceType.Sheep,
        ResourceTypeProto.Stone => ResourceType.Stone,
        ResourceTypeProto.Iron  => ResourceType.Iron,
        _ => throw new ArgumentOutOfRangeException(nameof(proto), proto, null),
    };

    // ─── enum mappings ───────────────────────────────────────────────────────

    private static GamePhaseProto MapGamePhase(Domain.GameFlow.GamePhase phase) => phase switch
    {
        Domain.GameFlow.GamePhase.MapGeneration    => GamePhaseProto.MapGeneration,
        Domain.GameFlow.GamePhase.MapVoting        => GamePhaseProto.MapVoting,
        Domain.GameFlow.GamePhase.InitialPlacement => GamePhaseProto.InitialPlacement,
        Domain.GameFlow.GamePhase.InProgress       => GamePhaseProto.InProgress,
        Domain.GameFlow.GamePhase.Ended            => GamePhaseProto.Ended,
        _ => GamePhaseProto.GamePhaseUnspecified,
    };

    private static TurnPhaseProto MapTurnPhase(Domain.GameFlow.TurnPhase phase) => phase switch
    {
        Domain.GameFlow.TurnPhase.WaitingForRoll   => TurnPhaseProto.WaitingForRoll,
        Domain.GameFlow.TurnPhase.ResourceProduction => TurnPhaseProto.ResourceProduction,
        Domain.GameFlow.TurnPhase.Discard          => TurnPhaseProto.Discard,
        Domain.GameFlow.TurnPhase.RobberMovement   => TurnPhaseProto.RobberMovement,
        Domain.GameFlow.TurnPhase.RobberTribute    => TurnPhaseProto.RobberTribute,
        Domain.GameFlow.TurnPhase.Trade            => TurnPhaseProto.Trade,
        Domain.GameFlow.TurnPhase.Build            => TurnPhaseProto.Build,
        Domain.GameFlow.TurnPhase.End              => TurnPhaseProto.End,
        _ => TurnPhaseProto.TurnPhaseUnspecified,
    };

    private static TileTypeProto MapTileType(TileType type) => type switch
    {
        TileType.Forest   => TileTypeProto.Forest,
        TileType.Quarry   => TileTypeProto.Quarry,
        TileType.Field    => TileTypeProto.Field,
        TileType.Pasture  => TileTypeProto.Pasture,
        TileType.Mountain => TileTypeProto.Mountain,
        TileType.Mine     => TileTypeProto.Mine,
        TileType.Desert   => TileTypeProto.Desert,
        _ => TileTypeProto.TileTypeUnspecified,
    };

    private static ResourceTypeProto MapResourceType(ResourceType type) => type switch
    {
        ResourceType.Wood  => ResourceTypeProto.Wood,
        ResourceType.Brick => ResourceTypeProto.Brick,
        ResourceType.Wheat => ResourceTypeProto.Wheat,
        ResourceType.Sheep => ResourceTypeProto.Sheep,
        ResourceType.Stone => ResourceTypeProto.Stone,
        ResourceType.Iron  => ResourceTypeProto.Iron,
        _ => ResourceTypeProto.ResourceTypeUnspecified,
    };
}
