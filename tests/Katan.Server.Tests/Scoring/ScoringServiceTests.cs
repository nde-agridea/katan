namespace Katan.Server.Tests.Scoring;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Military;
using Katan.Server.Domain.Scoring;
using Katan.Server.Domain.Scoring.Events;
using Katan.Server.Domain.Structures;
using FluentAssertions;

public class ScoringServiceTests
{
    private Game CreateGame(int players = 2)
    {
        var game = new Game();
        for (int i = 1; i <= players; i++)
            game.AddPlayer(new Player($"p{i}", $"Player {i}"));
        var board = new Domain.Board.Board();
        board.AddTile(new Tile(new TilePosition(0, 0), TileType.Forest, 6));
        board.PlaceRobber(new TilePosition(0, 0));
        game.SetBoard(board);
        game.StartGame("p1");
        return game;
    }

    // US-SC1: VP from settlements and cities
    [Fact]
    public void RecalculateVP_OneSettlement_GivesOneVP()
    {
        var game = CreateGame();
        var p1 = game.Players[0];
        p1.PlaceSettlement(new Settlement("p1", new Intersection(new[] { new TilePosition(0, 0) })));

        new ScoringService(game).RecalculateStructureVP();

        p1.VictoryPoints.Total.Should().Be(1);
    }

    [Fact]
    public void RecalculateVP_OneCity_GivesTwoVP()
    {
        var game = CreateGame();
        var p1 = game.Players[0];
        var intersection = new Intersection(new[] { new TilePosition(0, 0) });
        var settlement = new Settlement("p1", intersection);
        p1.PlaceSettlement(settlement);
        p1.PlaceCity(new City("p1", intersection), settlement);

        new ScoringService(game).RecalculateStructureVP();

        p1.VictoryPoints.Total.Should().Be(2);
    }

    // US-SC2: Longest Road
    [Fact]
    public void UpdateLongestRoad_PlayerWithLongerRoad_GetsBonus()
    {
        var game = CreateGame();
        var board = new Domain.Board.Board();
        // Build a grid with enough tiles for edges
        for (int q = -2; q <= 2; q++)
            for (int r = -2; r <= 2; r++)
                board.AddTile(new Tile(new TilePosition(q, r), TileType.Forest, 6));
        board.PlaceRobber(new TilePosition(0, 0));
        game.SetBoard(board);

        var p1 = game.Players[0];
        // In axial hex coords: two edges share an intersection if their tiles have a common neighbor.
        // Chain: (0,0)-(1,0) → (1,0)-(0,1) → (1,0)-(1,1) gives length 3.
        // These share intersections: {(0,0),(1,0),(0,1)} and {(1,0),(0,1),(1,1)}.
        p1.PlaceRoad(new Road("p1", new Edge(new TilePosition(0, 0), new TilePosition(1, 0))));
        p1.PlaceRoad(new Road("p1", new Edge(new TilePosition(1, 0), new TilePosition(0, 1))));
        p1.PlaceRoad(new Road("p1", new Edge(new TilePosition(1, 0), new TilePosition(1, 1))));

        var scoring = new ScoringService(game);
        int roadLength = scoring.ComputeLongestRoad(p1);

        roadLength.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void UpdateLongestRoad_SetsBonus_OnLeader()
    {
        var game = CreateGame();
        var board = new Domain.Board.Board();
        for (int q = -2; q <= 2; q++)
            for (int r = -2; r <= 2; r++)
                board.AddTile(new Tile(new TilePosition(q, r), TileType.Forest, 6));
        board.PlaceRobber(new TilePosition(0, 0));
        game.SetBoard(board);

        var p1 = game.Players[0];
        p1.PlaceRoad(new Road("p1", new Edge(new TilePosition(0, 0), new TilePosition(1, 0))));
        p1.PlaceRoad(new Road("p1", new Edge(new TilePosition(1, 0), new TilePosition(1, -1))));

        new ScoringService(game).UpdateLongestRoad();

        p1.VictoryPoints.HasLongestRoad.Should().BeTrue();
        game.DomainEvents.OfType<LongestRoadUpdated>().Should().HaveCount(1);
    }

    // US-SC3: Largest Army
    [Fact]
    public void UpdateLargestArmy_PlayerWithMostArmies_GetsBonus()
    {
        var game = CreateGame();
        var p1 = game.Players[0];
        var p2 = game.Players[1];

        p1.PlaceArmy(new Army("p1", new TilePosition(0, 0)));
        p1.PlaceArmy(new Army("p1", new TilePosition(0, 0)));
        p2.PlaceArmy(new Army("p2", new TilePosition(0, 0)));

        new ScoringService(game).UpdateLargestArmy();

        p1.VictoryPoints.HasLargestArmy.Should().BeTrue();
        p2.VictoryPoints.HasLargestArmy.Should().BeFalse();
        game.DomainEvents.OfType<LargestArmyUpdated>().Should().HaveCount(1);
    }

    // US-SC4: win condition
    [Fact]
    public void CheckWinCondition_PlayerAt10VP_EmitsGameEnded()
    {
        var game = CreateGame();
        var p1 = game.Players[0];

        // Give p1 10 VP manually via development card points
        for (int i = 0; i < 10; i++)
            p1.VictoryPoints.AddDevelopmentCardPoint();

        bool ended = new ScoringService(game).CheckWinCondition();

        ended.Should().BeTrue();
        game.Phase.Should().Be(GamePhase.Ended);
        game.DomainEvents.OfType<GameEnded>().Should().ContainSingle(e => e.WinnerId == "p1");
    }

    [Fact]
    public void CheckWinCondition_PlayerBelow10VP_ReturnsFalse()
    {
        var game = CreateGame();
        bool ended = new ScoringService(game).CheckWinCondition();
        ended.Should().BeFalse();
        game.Phase.Should().NotBe(GamePhase.Ended);
    }

    [Fact]
    public void VictoryPointLedger_Total_IncludesBonuses()
    {
        var ledger = new VictoryPointLedger();
        ledger.SetFromStructures(3);
        ledger.AddDevelopmentCardPoint();
        ledger.SetLongestRoad(true);
        ledger.SetLargestArmy(true);

        ledger.Total.Should().Be(3 + 1 + 2 + 2);
    }
}
