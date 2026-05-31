namespace Katan.Server.Tests.Board;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Resources.Events;
using Katan.Server.Domain.Structures;
using FluentAssertions;

public class RobberServiceTests
{
    private (Game game, RobberService service) CreateSetup()
    {
        var game = new Game();
        var board = new Board();
        var tile = new Tile(new TilePosition(0, 0), TileType.Forest, 6);
        board.AddTile(tile);
        board.PlaceRobber(new TilePosition(0, 0));
        game.SetBoard(board);

        var p1 = new Player("p1", "Player 1");
        var p2 = new Player("p2", "Player 2");
        game.AddPlayer(p1);
        game.AddPlayer(p2);

        var service = new RobberService(game);
        return (game, service);
    }

    [Fact]
    public void PayTribute_DeductsResourceFromPlayer()
    {
        var (game, service) = CreateSetup();
        var player = game.Players.First(p => p.Id == "p1");

        var settlement = new Settlement("p1", new Intersection(new[] { new TilePosition(0, 0) }));
        player.PlaceSettlement(settlement);
        player.Hand.Add(ResourceType.Wood, 2);

        service.PayTribute("p1", ResourceType.Wood);
        player.Hand.Count(ResourceType.Wood).Should().Be(1);
    }

    [Fact]
    public void PayTribute_EmitsRobberTributeCollectedEvent()
    {
        var (game, service) = CreateSetup();
        var player = game.Players.First(p => p.Id == "p1");
        var settlement = new Settlement("p1", new Intersection(new[] { new TilePosition(0, 0) }));
        player.PlaceSettlement(settlement);
        player.Hand.Add(ResourceType.Wood, 1);

        service.PayTribute("p1", ResourceType.Wood);
        game.DomainEvents.OfType<RobberTributeCollected>().Should().HaveCount(1);
    }

    [Fact]
    public void StealFromOpponent_TransfersResource()
    {
        var (game, service) = CreateSetup();
        var p1 = game.Players.First(p => p.Id == "p1");
        var p2 = game.Players.First(p => p.Id == "p2");

        var settlement = new Settlement("p2", new Intersection(new[] { new TilePosition(0, 0) }));
        p2.PlaceSettlement(settlement);
        p2.Hand.Add(ResourceType.Wood, 3);

        service.StealFromOpponent("p1", "p2");
        p2.Hand.Count(ResourceType.Wood).Should().Be(2);
        p1.Hand.Count(ResourceType.Wood).Should().Be(1);
    }
}
