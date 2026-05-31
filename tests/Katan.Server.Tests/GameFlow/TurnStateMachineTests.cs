namespace Katan.Server.Tests.GameFlow;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.GameFlow.Events;
using Katan.Server.Domain.Resources.Events;
using FluentAssertions;

public class TurnStateMachineTests
{
    private Game CreateGame(int playerCount = 2)
    {
        var game = new Game();
        for (int i = 1; i <= playerCount; i++)
            game.AddPlayer(new Player($"p{i}", $"Player {i}"));
        game.StartGame("p1");
        game.StartTurn();
        return game;
    }

    [Fact]
    public void RollDice_EmitsDiceRolledEvent()
    {
        var game = CreateGame();
        var sm = new TurnStateMachine(game, new Random(1));
        sm.RollDice();
        game.DomainEvents.OfType<DiceRolled>().Should().HaveCount(1);
    }

    [Fact]
    public void RollDice_WhenNotWaitingForRoll_Throws()
    {
        var game = CreateGame();
        var sm = new TurnStateMachine(game, new Random(1));
        sm.RollDice();
        var act = () => sm.RollDice();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TurnPhaseSequence_IsEnforced()
    {
        var game = CreateGame();
        var sm = new TurnStateMachine(game, new Random(1));
        var result = sm.RollDice();
        game.TurnPhase.Should().NotBe(TurnPhase.WaitingForRoll);
    }

    [Fact]
    public void EndTrade_AdvancesToBuildPhase()
    {
        var game = CreateGame();
        var sm = new TurnStateMachine(game, new Random(1));
        sm.RollDice();
        if (game.TurnPhase == TurnPhase.RobberTribute) sm.FinishRobberTribute();
        if (game.TurnPhase == TurnPhase.Discard) { sm.FinishDiscard(); sm.FinishRobberMovement(); }
        game.SetTurnPhase(TurnPhase.Trade);
        sm.EndTrade();
        game.TurnPhase.Should().Be(TurnPhase.Build);
    }

    [Fact]
    public void EndTurn_AdvancesActivePlayer()
    {
        var game = CreateGame(2);
        var sm = new TurnStateMachine(game, new Random(1));
        sm.RollDice();
        if (game.TurnPhase == TurnPhase.RobberTribute) sm.FinishRobberTribute();
        if (game.TurnPhase == TurnPhase.Discard) { sm.FinishDiscard(); sm.FinishRobberMovement(); }
        if (game.TurnPhase == TurnPhase.Trade) sm.EndTrade();
        if (game.TurnPhase == TurnPhase.Build) sm.EndBuild();
        sm.EndTurn();
        game.ActivePlayer!.Id.Should().Be("p2");
    }

    [Fact]
    public void SevenRoll_EmitsSevenRolledEvent()
    {
        var game = CreateGame();
        var sm = new TurnStateMachine(game, new FixedDiceRandom(3, 4));
        sm.RollDice();
        game.DomainEvents.OfType<SevenRolled>().Should().HaveCount(1);
        game.TurnPhase.Should().Be(TurnPhase.Discard);
    }
}

internal class FixedDiceRandom : Random
{
    private readonly Queue<int> _values;

    public FixedDiceRandom(params int[] values)
        => _values = new Queue<int>(values);

    public override int Next(int minValue, int maxValue) => _values.Dequeue();
}
