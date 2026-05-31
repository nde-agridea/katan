namespace Katan.Server.Tests.GameFlow;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Structures.Events;
using FluentAssertions;

public class InitialPlacementServiceTests
{
    [Fact]
    public void BuildPlacementOrder_CreatesSnakeOrder()
    {
        var game = CreateGame();
        var service = new InitialPlacementService(game);

        service.BuildPlacementOrder();
        service.CurrentPlayerId.Should().Be("p1");

        service.AdvancePlacement();
        service.CurrentPlayerId.Should().Be("p2");

        service.AdvancePlacement();
        service.CurrentPlayerId.Should().Be("p2");

        service.AdvancePlacement();
        service.CurrentPlayerId.Should().Be("p1");
    }

    [Fact]
    public void PlaceSettlement_GrantsStartingResourcesFromAdjacentNonDesertTiles()
    {
        var game = CreateGame();
        var board = new Katan.Server.Domain.Board.Board();
        board.AddTile(new Tile(new TilePosition(0, 0), TileType.Forest, 5));
        board.AddTile(new Tile(new TilePosition(1, 0), TileType.Desert, 7));
        game.SetBoard(board);

        var service = new InitialPlacementService(game);
        service.BuildPlacementOrder();

        service.PlaceSettlement("p1", new Intersection(new[] { new TilePosition(0, 0), new TilePosition(1, 0) }));

        var player = game.Players.Single(p => p.Id == "p1");
        player.Hand.Count(ResourceType.Wood).Should().Be(1);
        game.DomainEvents.OfType<StartingResourcesGranted>().Should().ContainSingle();
    }

    private static Game CreateGame()
    {
        var game = new Game();
        game.AddPlayer(new Player("p1", "Player 1"));
        game.AddPlayer(new Player("p2", "Player 2"));
        return game;
    }
}
