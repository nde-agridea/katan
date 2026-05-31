namespace Katan.Server.Domain.Military;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Military.Events;
using Katan.Server.Domain.Structures;

public class MilitaryService
{
    private readonly Game _game;
    private readonly Random _random;

    public MilitaryService(Game game, Random? random = null)
    {
        _game = game;
        _random = random ?? Random.Shared;
    }

    // US-M1/M2: move army to adjacent tile (owned, has player structure, or unclaimed)
    public void MoveArmy(string playerId, Army army, TilePosition destination)
    {
        if (army.PlayerId != playerId)
            throw new InvalidOperationException("Cannot move an army that belongs to another player.");

        var player = GetPlayer(playerId);
        if (!player.Armies.Contains(army))
            throw new InvalidOperationException("Player does not own this army.");

        var from = army.Position;
        if (!from.Neighbours().Contains(destination))
            throw new InvalidOperationException("Army can only move to an adjacent tile.");

        var destTile = _game.Board.GetTile(destination)
            ?? throw new InvalidOperationException("Destination tile does not exist.");

        bool canMoveTo = IsMovementAllowed(player, destTile);
        if (!canMoveTo)
            throw new InvalidOperationException("Army cannot move to that tile.");

        var previousPosition = army.Position;
        army.MoveTo(destination);

        // Claim unclaimed tile (US-M2)
        if (destTile.OwnerId is null)
        {
            destTile.Claim(playerId);
            _game.AddEvent(new TileClaimed(playerId, destination));
        }

        _game.AddEvent(new ArmyMoved(playerId, previousPosition, destination));
    }

    // US-M3: attack adjacent tile containing enemy armies
    public CombatResult Attack(string attackerId, TilePosition attackerTile, TilePosition defenderTile)
    {
        if (!attackerTile.Neighbours().Contains(defenderTile))
            throw new InvalidOperationException("Can only attack adjacent tiles.");

        var attackerPlayer = GetPlayer(attackerId);
        var attackingArmies = attackerPlayer.Armies.Where(a => a.Position == attackerTile).ToList();
        if (attackingArmies.Count == 0)
            throw new InvalidOperationException("No armies on attacking tile.");

        var tile = _game.Board.GetTile(defenderTile)
            ?? throw new InvalidOperationException("Defender tile does not exist.");
        if (tile.OwnerId is null || tile.OwnerId == attackerId)
            throw new InvalidOperationException("Can only attack a tile owned by an opponent.");

        var defenderPlayer = GetPlayer(tile.OwnerId);
        var defendingArmies = defenderPlayer.Armies.Where(a => a.Position == defenderTile).ToList();
        if (defendingArmies.Count == 0)
            throw new InvalidOperationException("No defending armies on target tile.");

        // US-M3: roll n dice each side, sum, ties go to defender
        int attackRoll = RollDice(attackingArmies.Count);
        int defenseRoll = RollDice(defendingArmies.Count);
        bool attackerWon = attackRoll > defenseRoll;

        _game.AddEvent(new CombatResolved(attackerId, defenderPlayer.Id, defenderTile, attackerWon, attackRoll, defenseRoll));

        if (attackerWon)
            ResolveAttackerWins(attackerPlayer, defenderPlayer, attackingArmies, defendingArmies, attackerTile, defenderTile, tile);
        else
            ResolveAttackerLoses(attackerPlayer, attackingArmies);

        return new CombatResult(attackerWon, attackRoll, defenseRoll);
    }

    // US-M4: attacker wins
    private void ResolveAttackerWins(
        GameFlow.Player attacker,
        GameFlow.Player defender,
        List<Army> attackingArmies,
        List<Army> defendingArmies,
        TilePosition attackerTile,
        TilePosition defenderTile,
        Tile tile)
    {
        // Remove all defending armies (return to supply)
        foreach (var army in defendingArmies)
            defender.ReturnArmyToSupply(army);

        // Transfer tile ownership
        var previousOwner = tile.OwnerId!;
        tile.TransferOwnership(attacker.Id);
        _game.AddEvent(new TileConquered(attacker.Id, previousOwner, defenderTile));

        // Transfer defender structures on the tile
        var defenderSettlements = defender.Settlements
            .Where(s => s.Location.AdjacentTiles.Contains(defenderTile)).ToList();
        foreach (var settlement in defenderSettlements)
        {
            defender.RemoveSettlement(settlement);
            var transferred = new Structures.Settlement(attacker.Id, settlement.Location);
            attacker.TransferSettlementIn(transferred);
        }

        var defenderCities = defender.Cities
            .Where(c => c.Location.AdjacentTiles.Contains(defenderTile)).ToList();
        foreach (var city in defenderCities)
        {
            defender.RemoveCity(city);
            var transferred = new Structures.City(attacker.Id, city.Location);
            attacker.TransferCityIn(transferred);
        }

        // Attacker must move at least 1 army onto conquered tile
        var firstArmy = attackingArmies[0];
        firstArmy.MoveTo(defenderTile);
        _game.AddEvent(new ArmyMoved(attacker.Id, attackerTile, defenderTile));
    }

    // US-M5: attacker loses – loses floor(attacking_armies / 2)
    private void ResolveAttackerLoses(GameFlow.Player attacker, List<Army> attackingArmies)
    {
        int losses = attackingArmies.Count / 2;
        for (int i = 0; i < losses; i++)
            attacker.ReturnArmyToSupply(attackingArmies[i]);
    }

    private bool IsMovementAllowed(GameFlow.Player player, Tile dest)
    {
        if (dest.OwnerId == player.Id) return true;
        if (dest.OwnerId is null) return true;
        bool hasStructure =
            player.Settlements.Any(s => s.Location.AdjacentTiles.Contains(dest.Position)) ||
            player.Cities.Any(c => c.Location.AdjacentTiles.Contains(dest.Position));
        return hasStructure;
    }

    private int RollDice(int count)
    {
        int total = 0;
        for (int i = 0; i < count; i++)
            total += _random.Next(1, 7);
        return total;
    }

    private GameFlow.Player GetPlayer(string id) =>
        _game.Players.FirstOrDefault(p => p.Id == id)
        ?? throw new InvalidOperationException($"Player '{id}' not found.");
}

public record CombatResult(bool AttackerWon, int AttackerRoll, int DefenderRoll);
