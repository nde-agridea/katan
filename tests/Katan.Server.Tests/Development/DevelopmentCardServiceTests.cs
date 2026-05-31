namespace Katan.Server.Tests.Development;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.Development;
using Katan.Server.Domain.Development.Events;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Resources;
using Katan.Server.Domain.Structures;
using FluentAssertions;

public class DevelopmentCardServiceTests
{
    private Game CreateGame()
    {
        var game = new Game();
        game.AddPlayer(new Player("p1", "Player 1"));
        game.AddPlayer(new Player("p2", "Player 2"));

        var board = new Domain.Board.Board();
        board.AddTile(new Tile(new TilePosition(0, 0), TileType.Forest, 6));
        board.AddTile(new Tile(new TilePosition(1, 0), TileType.Forest, 5));
        board.PlaceRobber(new TilePosition(0, 0));
        game.SetBoard(board);
        return game;
    }

    // US-D1: purchase dev card
    [Fact]
    public void PurchaseAndPlay_DeductsResources()
    {
        var game = CreateGame();
        var p1 = game.Players[0];
        p1.Hand.Add(ResourceType.Stone, 1);
        p1.Hand.Add(ResourceType.Sheep, 1);
        p1.Hand.Add(ResourceType.Iron, 1);

        var deck = new DevelopmentCardDeck(new Random(1));
        var svc = new DevelopmentCardService(game, deck);
        svc.PurchaseAndPlay("p1");

        p1.Hand.Total.Should().Be(0);
        game.DomainEvents.OfType<DevelopmentCardPurchased>().Should().HaveCount(1);
    }

    [Fact]
    public void PurchaseAndPlay_InsufficientResources_Throws()
    {
        var game = CreateGame();
        var svc = new DevelopmentCardService(game);
        var act = () => svc.PurchaseAndPlay("p1");
        act.Should().Throw<InvalidOperationException>();
    }

    // US-D2: knight
    [Fact]
    public void PlayKnight_MovesRobber()
    {
        var game = CreateGame();
        var svc = new DevelopmentCardService(game);
        var newPos = new TilePosition(1, 0);

        svc.PlayKnight("p1", newPos);

        game.Board.RobberPosition.Should().Be(newPos);
        game.DomainEvents.OfType<KnightPlayed>().Should().HaveCount(1);
    }

    // US-D3: victory point
    [Fact]
    public void PlayVictoryPoint_AddsOneVP()
    {
        var game = CreateGame();
        var svc = new DevelopmentCardService(game);
        svc.PlayVictoryPoint("p1");

        game.Players[0].VictoryPoints.Total.Should().Be(1);
    }

    // US-D5: monopoly
    [Fact]
    public void PlayMonopoly_TakesOneResourceFromEachOpponent()
    {
        var game = CreateGame();
        game.Players[1].Hand.Add(ResourceType.Wood, 3);
        var svc = new DevelopmentCardService(game);

        svc.PlayMonopoly("p1", ResourceType.Wood);

        game.Players[0].Hand.Count(ResourceType.Wood).Should().Be(1);
        game.Players[1].Hand.Count(ResourceType.Wood).Should().Be(2);
    }

    [Fact]
    public void PlayMonopoly_OpponentHasNone_GivesNothing()
    {
        var game = CreateGame();
        var svc = new DevelopmentCardService(game);
        svc.PlayMonopoly("p1", ResourceType.Wood);
        game.Players[0].Hand.Count(ResourceType.Wood).Should().Be(0);
    }

    // US-D6: excess
    [Fact]
    public void PlayExcess_GivesTwoResources()
    {
        var game = CreateGame();
        var svc = new DevelopmentCardService(game);
        svc.PlayExcess("p1", ResourceType.Wood, ResourceType.Brick);

        game.Players[0].Hand.Count(ResourceType.Wood).Should().Be(1);
        game.Players[0].Hand.Count(ResourceType.Brick).Should().Be(1);
    }

    // US-D7: disaster
    [Fact]
    public void PlayDisaster_DestroysSettlementOnTile()
    {
        var game = CreateGame();
        var p2 = game.Players[1];
        var intersection = new Intersection(new[] { new TilePosition(0, 0) });
        p2.PlaceSettlement(new Settlement("p2", intersection));

        var svc = new DevelopmentCardService(game);
        svc.PlayDisaster("p1", new TilePosition(0, 0));

        p2.Settlements.Should().BeEmpty();
    }

    [Fact]
    public void PlayDisaster_HalvesArmiesOnTile()
    {
        var game = CreateGame();
        var p2 = game.Players[1];
        p2.PlaceArmy(new Domain.Military.Army("p2", new TilePosition(0, 0)));
        p2.PlaceArmy(new Domain.Military.Army("p2", new TilePosition(0, 0)));
        p2.PlaceArmy(new Domain.Military.Army("p2", new TilePosition(0, 0)));
        p2.PlaceArmy(new Domain.Military.Army("p2", new TilePosition(0, 0)));

        var svc = new DevelopmentCardService(game);
        svc.PlayDisaster("p1", new TilePosition(0, 0));

        p2.Armies.Count.Should().Be(2); // floor(4/2) = 2 lost
    }

    // Deck reshuffle
    [Fact]
    public void Draw_WhenDeckExhausted_Reshuffles()
    {
        var deck = new DevelopmentCardDeck(new Random(42));
        // Draw all 25 cards
        for (int i = 0; i < 25; i++) deck.Draw();
        deck.Count.Should().Be(0);

        // Drawing again should reshuffle
        var card = deck.Draw();
        card.Should().NotBeNull();
    }
}
