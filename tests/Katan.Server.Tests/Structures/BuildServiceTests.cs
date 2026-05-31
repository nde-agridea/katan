namespace Katan.Server.Tests.Structures;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Resources;
using Katan.Server.Domain.Structures;
using Katan.Server.Domain.Structures.Events;
using FluentAssertions;

public class BuildServiceTests
{
    private Game CreateGameInBuildPhase()
    {
        var game = new Game();
        var p1 = new Player("p1", "Player 1");
        game.AddPlayer(p1);

        var board = BuildBoardWithTiles();
        game.SetBoard(board);

        game.StartGame("p1");
        game.StartTurn();
        game.SetTurnPhase(TurnPhase.Build);
        return game;
    }

    private static Domain.Board.Board BuildBoardWithTiles()
    {
        var board = new Domain.Board.Board();
        // Place a 3x3 patch so edges have common neighbours
        int[] qs = { 0, 1, -1, 0, 1, -1, 0, 1, -1 };
        int[] rs = { 0, 0, 0, 1, 1, 1, -1, -1, -1 };
        for (int i = 0; i < qs.Length; i++)
            board.AddTile(new Tile(new TilePosition(qs[i], rs[i]), TileType.Forest, 6));
        board.PlaceRobber(new TilePosition(0, 0));
        return board;
    }

    // US-BU1: road building
    [Fact]
    public void BuildRoad_WithCostAndConnectivity_Succeeds()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Wood, 1);
        player.Hand.Add(ResourceType.Brick, 1);

        // Place a settlement so we have a network to connect to
        var intersection = new Intersection(new[] { new TilePosition(0, 0), new TilePosition(1, 0), new TilePosition(0, 1) });
        player.PlaceSettlement(new Settlement("p1", intersection));

        // The road is on an edge sharing that intersection
        var edge = new Edge(new TilePosition(0, 0), new TilePosition(1, 0));

        new BuildService(game).BuildRoad("p1", edge);

        player.Roads.Should().HaveCount(1);
        player.RemainingRoads.Should().Be(14);
        player.Hand.Count(ResourceType.Wood).Should().Be(0);
        game.DomainEvents.OfType<RoadBuilt>().Should().HaveCount(1);
    }

    [Fact]
    public void BuildRoad_NoConnectivity_Throws()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Wood, 1);
        player.Hand.Add(ResourceType.Brick, 1);

        var edge = new Edge(new TilePosition(0, 0), new TilePosition(1, 0));
        var act = () => new BuildService(game).BuildRoad("p1", edge);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BuildRoad_InsufficientResources_Throws()
    {
        var game = CreateGameInBuildPhase();
        var edge = new Edge(new TilePosition(0, 0), new TilePosition(1, 0));
        var act = () => new BuildService(game).BuildRoad("p1", edge);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BuildRoad_PieceLimitReached_Throws()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];

        // Exhaust all roads
        for (int i = 0; i < 15; i++)
            player.PlaceRoad(new Road("p1", new Edge(new TilePosition(0, 0))));

        player.Hand.Add(ResourceType.Wood, 1);
        player.Hand.Add(ResourceType.Brick, 1);

        var edge = new Edge(new TilePosition(0, 0), new TilePosition(1, 0));
        var act = () => new BuildService(game).BuildRoad("p1", edge);
        act.Should().Throw<InvalidOperationException>().WithMessage("*piece limit*");
    }

    // US-BU2: settlement building
    [Fact]
    public void BuildSettlement_WithRoadConnectivity_Succeeds()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Wood, 2);
        player.Hand.Add(ResourceType.Brick, 1);
        player.Hand.Add(ResourceType.Wheat, 1);
        player.Hand.Add(ResourceType.Sheep, 1);

        var settlementIntersection = new Intersection(new[] { new TilePosition(0, 0), new TilePosition(1, 0), new TilePosition(0, 1) });
        var roadEdge = new Edge(new TilePosition(0, 0), new TilePosition(1, 0));
        player.PlaceRoad(new Road("p1", roadEdge));

        new BuildService(game).BuildSettlement("p1", settlementIntersection);

        player.Settlements.Should().HaveCount(1);
        player.RemainingSettlements.Should().Be(4);
        game.DomainEvents.OfType<SettlementBuilt>().Should().HaveCount(1);
    }

    [Fact]
    public void BuildSettlement_NoRoadConnection_Throws()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Wood, 2);
        player.Hand.Add(ResourceType.Brick, 1);
        player.Hand.Add(ResourceType.Wheat, 1);
        player.Hand.Add(ResourceType.Sheep, 1);

        var intersection = new Intersection(new[] { new TilePosition(0, 0), new TilePosition(1, 0), new TilePosition(0, 1) });
        var act = () => new BuildService(game).BuildSettlement("p1", intersection);
        act.Should().Throw<InvalidOperationException>();
    }

    // US-BU3: city upgrade
    [Fact]
    public void UpgradeCity_ReplacesSettlement()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Stone, 3);
        player.Hand.Add(ResourceType.Wheat, 2);

        var intersection = new Intersection(new[] { new TilePosition(0, 0), new TilePosition(1, 0), new TilePosition(0, 1) });
        player.PlaceSettlement(new Settlement("p1", intersection));

        new BuildService(game).UpgradeCity("p1", intersection);

        player.Cities.Should().HaveCount(1);
        player.Settlements.Should().BeEmpty();
        player.RemainingSettlements.Should().Be(5); // returned to supply
        game.DomainEvents.OfType<CityUpgraded>().Should().HaveCount(1);
    }

    [Fact]
    public void UpgradeCity_NoExistingSettlement_Throws()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Stone, 3);
        player.Hand.Add(ResourceType.Wheat, 2);

        var intersection = new Intersection(new[] { new TilePosition(0, 0) });
        var act = () => new BuildService(game).UpgradeCity("p1", intersection);
        act.Should().Throw<InvalidOperationException>();
    }

    // US-BU4: army building
    [Fact]
    public void BuildArmy_OnTileWithSettlement_Succeeds()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Iron, 2);
        player.Hand.Add(ResourceType.Wheat, 1);

        var tilePos = new TilePosition(0, 0);
        var intersection = new Intersection(new[] { tilePos });
        player.PlaceSettlement(new Settlement("p1", intersection));

        new BuildService(game).BuildArmy("p1", tilePos);

        player.Armies.Should().HaveCount(1);
    }

    [Fact]
    public void BuildArmy_NoStructureOnTile_Throws()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        player.Hand.Add(ResourceType.Iron, 2);
        player.Hand.Add(ResourceType.Wheat, 1);

        var act = () => new BuildService(game).BuildArmy("p1", new TilePosition(0, 0));
        act.Should().Throw<InvalidOperationException>();
    }

    // US-BU5: piece limits
    [Fact]
    public void BuildSettlement_PieceLimitReached_Throws()
    {
        var game = CreateGameInBuildPhase();
        var player = game.Players[0];
        for (int i = 0; i < 5; i++)
            player.PlaceSettlement(new Settlement("p1", new Intersection(new[] { new TilePosition(i, 0) })));

        player.Hand.Add(ResourceType.Wood, 2);
        player.Hand.Add(ResourceType.Brick, 1);
        player.Hand.Add(ResourceType.Wheat, 1);
        player.Hand.Add(ResourceType.Sheep, 1);

        var intersection = new Intersection(new[] { new TilePosition(0, 0), new TilePosition(1, 0), new TilePosition(0, 1) });
        var act = () => new BuildService(game).BuildSettlement("p1", intersection, skipConnectivity: true);
        act.Should().Throw<InvalidOperationException>().WithMessage("*piece limit*");
    }
}
