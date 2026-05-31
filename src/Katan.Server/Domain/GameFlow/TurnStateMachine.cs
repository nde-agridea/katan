namespace Katan.Server.Domain.GameFlow;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow.Events;
using Katan.Server.Domain.Resources.Events;

public class TurnStateMachine
{
    private readonly Game _game;
    private readonly Random _random;

    public TurnStateMachine(Game game, Random? random = null)
    {
        _game = game;
        _random = random ?? Random.Shared;
    }

    public DiceResult RollDice()
    {
        if (_game.TurnPhase != TurnPhase.WaitingForRoll)
            throw new InvalidOperationException("Dice can only be rolled at the start of a turn.");

        var result = new DiceResult(_random.Next(1, 7), _random.Next(1, 7));
        _game.AddEvent(new DiceRolled(_game.ActivePlayer!.Id, result));

        if (result.IsSeven)
        {
            _game.SetTurnPhase(TurnPhase.Discard);
            _game.AddEvent(new SevenRolled(_game.ActivePlayer!.Id));
        }
        else
        {
            ProduceResources(result.Total);
            CollectRobberTribute();
            if (_game.TurnPhase != TurnPhase.RobberTribute)
                _game.SetTurnPhase(TurnPhase.Trade);
        }

        return result;
    }

    private void ProduceResources(int total)
    {
        var robberPosition = _game.Board.RobberPosition;
        foreach (var tile in _game.Board.GetTilesWithToken(total))
        {
            if (tile.Position == robberPosition)
                continue;

            foreach (var player in _game.Players)
            {
                var settlementCount = player.Settlements.Count(settlement => settlement.Location.AdjacentTiles.Contains(tile.Position));
                var cityCount = player.Cities.Count(city => city.Location.AdjacentTiles.Contains(tile.Position));
                var amount = settlementCount + (cityCount * 2);
                if (amount > 0 && tile.Produces is { } resource)
                {
                    player.Hand.Add(resource, amount);
                    _game.AddEvent(new ResourcesProduced(player.Id, resource, amount));
                }
            }
        }
    }

    private void CollectRobberTribute()
    {
        var robberPosition = _game.Board.RobberPosition;
        if (robberPosition is null)
            return;

        _game.SetTurnPhase(TurnPhase.RobberTribute);
    }

    public void FinishRobberTribute()
    {
        _game.SetTurnPhase(TurnPhase.Trade);
    }

    public void FinishDiscard()
    {
        _game.SetTurnPhase(TurnPhase.RobberMovement);
    }

    public void FinishRobberMovement()
    {
        _game.SetTurnPhase(TurnPhase.Trade);
    }

    public void EndTrade()
    {
        if (_game.TurnPhase != TurnPhase.Trade)
            throw new InvalidOperationException("Not in Trade phase.");

        _game.SetTurnPhase(TurnPhase.Build);
    }

    public void EndBuild()
    {
        if (_game.TurnPhase != TurnPhase.Build)
            throw new InvalidOperationException("Not in Build phase.");

        _game.SetTurnPhase(TurnPhase.End);
    }

    public void EndTurn()
    {
        if (_game.TurnPhase != TurnPhase.End)
            throw new InvalidOperationException("Turn must be in End phase to end.");

        _game.AdvanceTurnToNextPlayer();
    }
}