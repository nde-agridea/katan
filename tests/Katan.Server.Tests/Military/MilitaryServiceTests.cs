namespace Katan.Server.Tests.Military;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Military;
using Katan.Server.Domain.Military.Events;
using Katan.Server.Domain.Structures;
using FluentAssertions;

public class MilitaryServiceTests
{
    private (Game game, MilitaryService service) CreateSetup(Random? random = null)
    {
        var game = new Game();
        game.AddPlayer(new Player("p1", "Player 1"));
        game.AddPlayer(new Player("p2", "Player 2"));

        var board = new Domain.Board.Board();
        var tileA = new TilePosition(0, 0);
        var tileB = new TilePosition(1, 0); // adjacent to A
        board.AddTile(new Tile(tileA, TileType.Forest, 6));
        board.AddTile(new Tile(tileB, TileType.Forest, 5));
        board.PlaceRobber(tileA);
        game.SetBoard(board);

        return (game, new MilitaryService(game, random));
    }

    // US-M1/M2: army movement
    [Fact]
    public void MoveArmy_ToUnclaimed_Succeeds()
    {
        var (game, svc) = CreateSetup();
        var p1 = game.Players[0];

        var army = new Army("p1", new TilePosition(0, 0));
        p1.PlaceArmy(army);
        game.Board.GetTile(new TilePosition(0, 0))!.Claim("p1");

        svc.MoveArmy("p1", army, new TilePosition(1, 0));

        army.Position.Should().Be(new TilePosition(1, 0));
        game.DomainEvents.OfType<TileClaimed>().Should().HaveCount(1);
        game.DomainEvents.OfType<ArmyMoved>().Should().HaveCount(1);
    }

    [Fact]
    public void MoveArmy_ToOwnedTile_Succeeds()
    {
        var (game, svc) = CreateSetup();
        var p1 = game.Players[0];

        game.Board.GetTile(new TilePosition(0, 0))!.Claim("p1");
        game.Board.GetTile(new TilePosition(1, 0))!.Claim("p1");

        var army = new Army("p1", new TilePosition(0, 0));
        p1.PlaceArmy(army);

        svc.MoveArmy("p1", army, new TilePosition(1, 0));
        army.Position.Should().Be(new TilePosition(1, 0));
    }

    [Fact]
    public void MoveArmy_ToEnemyTileWithoutStructure_Throws()
    {
        var (game, svc) = CreateSetup();
        var p1 = game.Players[0];

        game.Board.GetTile(new TilePosition(0, 0))!.Claim("p1");
        game.Board.GetTile(new TilePosition(1, 0))!.Claim("p2"); // enemy owned, no player structure

        var army = new Army("p1", new TilePosition(0, 0));
        p1.PlaceArmy(army);

        var act = () => svc.MoveArmy("p1", army, new TilePosition(1, 0));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MoveArmy_ToNonAdjacentTile_Throws()
    {
        var (game, svc) = CreateSetup();
        var p1 = game.Players[0];
        game.Board.GetTile(new TilePosition(0, 0))!.Claim("p1");
        var army = new Army("p1", new TilePosition(0, 0));
        p1.PlaceArmy(army);

        var farTile = new TilePosition(5, 5);
        var act = () => svc.MoveArmy("p1", army, farTile);
        act.Should().Throw<InvalidOperationException>();
    }

    // US-M3: combat – attacker wins (use a seeded random that gives attacker higher roll)
    [Fact]
    public void Attack_AttackerWins_TileConquered()
    {
        // Use fixed random: attacker rolls 6, defender rolls 1
        var (game, _) = CreateSetup();
        var svc = new MilitaryService(game, new FixedSequenceRandom(6, 1)); // attacker=6, defender=1

        var p1 = game.Players[0];
        var p2 = game.Players[1];

        game.Board.GetTile(new TilePosition(0, 0))!.Claim("p1");
        game.Board.GetTile(new TilePosition(1, 0))!.Claim("p2");

        // p1 has 1 army attacking, p2 has 1 army defending
        p1.PlaceArmy(new Army("p1", new TilePosition(0, 0)));
        p2.PlaceArmy(new Army("p2", new TilePosition(1, 0)));

        var result = svc.Attack("p1", new TilePosition(0, 0), new TilePosition(1, 0));

        result.AttackerWon.Should().BeTrue();
        game.Board.GetTile(new TilePosition(1, 0))!.OwnerId.Should().Be("p1");
        game.DomainEvents.OfType<TileConquered>().Should().HaveCount(1);
    }

    // US-M5: attacker loses
    [Fact]
    public void Attack_AttackerLoses_HalfArmiesLost()
    {
        var (game, _) = CreateSetup();
        var svc = new MilitaryService(game, new FixedSequenceRandom(1, 6)); // attacker=1, defender=6

        var p1 = game.Players[0];
        var p2 = game.Players[1];

        game.Board.GetTile(new TilePosition(0, 0))!.Claim("p1");
        game.Board.GetTile(new TilePosition(1, 0))!.Claim("p2");

        // p1 has 2 attacking armies, p2 has 1 defending
        p1.PlaceArmy(new Army("p1", new TilePosition(0, 0)));
        p1.PlaceArmy(new Army("p1", new TilePosition(0, 0)));
        p2.PlaceArmy(new Army("p2", new TilePosition(1, 0)));

        // Roll: attacker=1+1=2, defender=6
        var svc2 = new MilitaryService(game, new FixedSequenceRandom(1, 1, 6)); // two attacker rolls, one defender
        var result = svc2.Attack("p1", new TilePosition(0, 0), new TilePosition(1, 0));

        result.AttackerWon.Should().BeFalse();
        // 2 attacking armies, floor(2/2)=1 lost → 1 remaining
        p1.Armies.Count.Should().Be(1);
        p1.RemainingArmies.Should().Be(9); // 1 returned to supply
    }
}

internal class FixedSequenceRandom : Random
{
    private readonly Queue<int> _values;
    public FixedSequenceRandom(params int[] values) => _values = new Queue<int>(values);
    public override int Next(int minValue, int maxValue) =>
        _values.Count > 0 ? _values.Dequeue() : minValue;
}
