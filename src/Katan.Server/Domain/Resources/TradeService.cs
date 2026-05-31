namespace Katan.Server.Domain.Resources;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Resources.Events;

public class TradeService
{
    private readonly Game _game;

    public TradeService(Game game)
    {
        _game = game;
    }

    // US-TR1: bank trade 4:1
    public void BankTrade(string playerId, ResourceType give, int amount, ResourceType receive)
    {
        const int bankRatio = 4;
        if (amount != bankRatio) throw new InvalidOperationException($"Bank trade requires exactly {bankRatio} identical resources.");
        if (give == receive) throw new InvalidOperationException("Cannot trade a resource for itself.");

        var player = GetPlayer(playerId);
        if (!player.Hand.CanAfford(new Dictionary<ResourceType, int> { [give] = amount }))
            throw new InvalidOperationException($"Insufficient {give} for bank trade.");

        player.Hand.Pay(new Dictionary<ResourceType, int> { [give] = amount });
        player.Hand.Add(receive, 1);
        _game.AddEvent(new BankTradeExecuted(playerId, give, amount, receive));
    }

    // US-TR2 / US-TR3: port trade
    public void PortTrade(string playerId, ResourceType give, int amount, ResourceType receive)
    {
        if (give == receive) throw new InvalidOperationException("Cannot trade a resource for itself.");

        var player = GetPlayer(playerId);
        int ratio = GetBestPortRatio(player, give);

        if (amount != ratio) throw new InvalidOperationException($"Port trade requires exactly {ratio} {give} resources.");
        if (!player.Hand.CanAfford(new Dictionary<ResourceType, int> { [give] = amount }))
            throw new InvalidOperationException($"Insufficient {give} for port trade.");

        player.Hand.Pay(new Dictionary<ResourceType, int> { [give] = amount });
        player.Hand.Add(receive, 1);
        _game.AddEvent(new PortTradeExecuted(playerId, give, amount, receive));
    }

    // Returns the best trade ratio for the given resource type (bank 4:1, generic port 3:1, specialized 2:1)
    private int GetBestPortRatio(GameFlow.Player player, ResourceType give)
    {
        int best = 4; // bank default

        var playerTiles = player.Settlements.SelectMany(s => s.Location.AdjacentTiles)
            .Concat(player.Cities.SelectMany(c => c.Location.AdjacentTiles))
            .ToHashSet();

        foreach (var port in _game.Board.Ports)
        {
            // Port is accessible if player has a settlement/city on the tile the port is on
            bool accessible = port.CoastalEdge.AdjacentTiles.Any(t => playerTiles.Contains(t));
            if (!accessible) continue;

            if (port.PortType == PortType.Generic && best > 3)
                best = 3;
            else if (port.PortType == PortType.Specialized && port.ResourceType == give && best > 2)
                best = 2;
        }
        return best;
    }

    // US-TR4: player-to-player trade offer
    public void OfferTrade(
        string offeringPlayerId,
        string targetPlayerId,
        IReadOnlyDictionary<ResourceType, int> offering,
        IReadOnlyDictionary<ResourceType, int> requesting)
    {
        if (offeringPlayerId == targetPlayerId) throw new InvalidOperationException("Cannot trade with yourself.");

        var offerer = GetPlayer(offeringPlayerId);
        if (!offerer.Hand.CanAfford(offering))
            throw new InvalidOperationException("Offering player cannot afford the offered resources.");

        _game.SetPendingTrade(new PendingTrade(offeringPlayerId, targetPlayerId, offering, requesting));
        _game.AddEvent(new TradeOffered(offeringPlayerId, targetPlayerId, offering, requesting));
    }

    public void AcceptTrade(string offeringPlayerId, string acceptingPlayerId)
    {
        var pending = _game.PendingTrade;
        if (pending is null) throw new InvalidOperationException("No pending trade.");
        if (pending.OfferingPlayerId != offeringPlayerId || pending.TargetPlayerId != acceptingPlayerId)
            throw new InvalidOperationException("No matching trade offer.");

        var offerer = GetPlayer(offeringPlayerId);
        var accepter = GetPlayer(acceptingPlayerId);

        if (!offerer.Hand.CanAfford(pending.Offering))
            throw new InvalidOperationException("Offering player can no longer afford the offered resources.");
        if (!accepter.Hand.CanAfford(pending.Requesting))
            throw new InvalidOperationException("Accepting player cannot afford the requested resources.");

        offerer.Hand.Pay(pending.Offering);
        accepter.Hand.Pay(pending.Requesting);
        foreach (var (type, amt) in pending.Requesting) offerer.Hand.Add(type, amt);
        foreach (var (type, amt) in pending.Offering) accepter.Hand.Add(type, amt);

        _game.AddEvent(new TradeAccepted(offeringPlayerId, acceptingPlayerId));
        _game.SetPendingTrade(null);
    }

    public void DeclineTrade(string offeringPlayerId, string decliningPlayerId)
    {
        var pending = _game.PendingTrade;
        if (pending is null) throw new InvalidOperationException("No pending trade.");
        if (pending.OfferingPlayerId != offeringPlayerId || pending.TargetPlayerId != decliningPlayerId)
            throw new InvalidOperationException("No matching trade offer.");

        _game.AddEvent(new TradeDeclined(offeringPlayerId, decliningPlayerId));
        _game.SetPendingTrade(null);
    }

    private GameFlow.Player GetPlayer(string id) =>
        _game.Players.FirstOrDefault(p => p.Id == id)
        ?? throw new InvalidOperationException($"Player '{id}' not found.");
}
