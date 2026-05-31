namespace Katan.Server.Tests.Resources;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Resources;
using Katan.Server.Domain.Resources.Events;
using Katan.Server.Domain.Structures;
using FluentAssertions;

public class TradeServiceTests
{
    private Game CreateGameWithPlayers()
    {
        var game = new Game();
        game.AddPlayer(new Player("p1", "Player 1"));
        game.AddPlayer(new Player("p2", "Player 2"));
        var board = new Domain.Board.Board();
        board.AddTile(new Tile(new TilePosition(0, 0), TileType.Forest, 6));
        game.SetBoard(board);
        return game;
    }

    // US-TR1: bank trade 4:1
    [Fact]
    public void BankTrade_ExchangesFourForOne()
    {
        var game = CreateGameWithPlayers();
        var p1 = game.Players[0];
        p1.Hand.Add(ResourceType.Wood, 4);

        new TradeService(game).BankTrade("p1", ResourceType.Wood, 4, ResourceType.Brick);

        p1.Hand.Count(ResourceType.Wood).Should().Be(0);
        p1.Hand.Count(ResourceType.Brick).Should().Be(1);
    }

    [Fact]
    public void BankTrade_WrongAmount_Throws()
    {
        var game = CreateGameWithPlayers();
        game.Players[0].Hand.Add(ResourceType.Wood, 3);
        var act = () => new TradeService(game).BankTrade("p1", ResourceType.Wood, 3, ResourceType.Brick);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BankTrade_InsufficientResources_Throws()
    {
        var game = CreateGameWithPlayers();
        var act = () => new TradeService(game).BankTrade("p1", ResourceType.Wood, 4, ResourceType.Brick);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void BankTrade_EmitsBankTradeExecutedEvent()
    {
        var game = CreateGameWithPlayers();
        game.Players[0].Hand.Add(ResourceType.Wood, 4);
        new TradeService(game).BankTrade("p1", ResourceType.Wood, 4, ResourceType.Brick);
        game.DomainEvents.OfType<BankTradeExecuted>().Should().HaveCount(1);
    }

    // US-TR2: generic port 3:1
    [Fact]
    public void PortTrade_WithGenericPort_ExchangesThreeForOne()
    {
        var game = CreateGameWithPlayers();
        var p1 = game.Players[0];

        var tile = new TilePosition(0, 0);
        var intersection = new Intersection(new[] { tile });
        p1.PlaceSettlement(new Settlement("p1", intersection));

        var port = new Port(new Edge(tile), PortType.Generic);
        game.Board.AddPort(port);

        p1.Hand.Add(ResourceType.Wood, 3);

        new TradeService(game).PortTrade("p1", ResourceType.Wood, 3, ResourceType.Brick);

        p1.Hand.Count(ResourceType.Wood).Should().Be(0);
        p1.Hand.Count(ResourceType.Brick).Should().Be(1);
    }

    // US-TR3: specialized port 2:1
    [Fact]
    public void PortTrade_WithSpecializedPort_ExchangesTwoForOne()
    {
        var game = CreateGameWithPlayers();
        var p1 = game.Players[0];

        var tile = new TilePosition(0, 0);
        var intersection = new Intersection(new[] { tile });
        p1.PlaceSettlement(new Settlement("p1", intersection));

        var port = new Port(new Edge(tile), PortType.Specialized, ResourceType.Wood);
        game.Board.AddPort(port);

        p1.Hand.Add(ResourceType.Wood, 2);

        new TradeService(game).PortTrade("p1", ResourceType.Wood, 2, ResourceType.Brick);

        p1.Hand.Count(ResourceType.Wood).Should().Be(0);
        p1.Hand.Count(ResourceType.Brick).Should().Be(1);
    }

    // US-TR4: player-to-player trade
    [Fact]
    public void OfferAndAcceptTrade_TransfersResources()
    {
        var game = CreateGameWithPlayers();
        var p1 = game.Players[0];
        var p2 = game.Players[1];
        p1.Hand.Add(ResourceType.Wood, 2);
        p2.Hand.Add(ResourceType.Brick, 1);

        var svc = new TradeService(game);
        svc.OfferTrade("p1", "p2",
            new Dictionary<ResourceType, int> { [ResourceType.Wood] = 2 },
            new Dictionary<ResourceType, int> { [ResourceType.Brick] = 1 });
        svc.AcceptTrade("p1", "p2");

        p1.Hand.Count(ResourceType.Wood).Should().Be(0);
        p1.Hand.Count(ResourceType.Brick).Should().Be(1);
        p2.Hand.Count(ResourceType.Wood).Should().Be(2);
        p2.Hand.Count(ResourceType.Brick).Should().Be(0);
    }

    [Fact]
    public void DeclineTrade_EmitsTradeDeclinedEvent()
    {
        var game = CreateGameWithPlayers();
        game.Players[0].Hand.Add(ResourceType.Wood, 1);

        var svc = new TradeService(game);
        svc.OfferTrade("p1", "p2",
            new Dictionary<ResourceType, int> { [ResourceType.Wood] = 1 },
            new Dictionary<ResourceType, int>());
        svc.DeclineTrade("p1", "p2");

        game.DomainEvents.OfType<TradeDeclined>().Should().HaveCount(1);
    }
}
