namespace Katan.Server.Domain.Scoring;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.GameFlow;
using Katan.Server.Domain.Scoring.Events;
using Katan.Server.Domain.Structures;

public class ScoringService
{
    private readonly Game _game;

    public ScoringService(Game game)
    {
        _game = game;
    }

    /// <summary>US-SC1: recompute structure VP for all players.</summary>
    public void RecalculateStructureVP()
    {
        foreach (var player in _game.Players)
        {
            int vp = player.Settlements.Count + player.Cities.Count * 2;
            player.VictoryPoints.SetFromStructures(vp);
            _game.AddEvent(new VictoryPointsUpdated(player.Id, player.VictoryPoints.Total));
        }
    }

    /// <summary>US-SC2: find the player with the longest continuous road and update bonuses.</summary>
    public void UpdateLongestRoad()
    {
        string? leaderId = null;
        int maxLength = 0;

        foreach (var player in _game.Players)
        {
            int length = ComputeLongestRoad(player);
            if (length > maxLength)
            {
                maxLength = length;
                leaderId = player.Id;
            }
        }

        bool changed = false;
        foreach (var player in _game.Players)
        {
            bool shouldHave = player.Id == leaderId && maxLength > 0;
            if (player.VictoryPoints.HasLongestRoad != shouldHave)
            {
                player.VictoryPoints.SetLongestRoad(shouldHave);
                changed = true;
            }
        }

        if (changed)
            _game.AddEvent(new LongestRoadUpdated(leaderId, maxLength));
    }

    /// <summary>US-SC3: find the player with the largest army and update bonuses.</summary>
    public void UpdateLargestArmy()
    {
        string? leaderId = null;
        int maxCount = 0;

        foreach (var player in _game.Players)
        {
            int count = player.Armies.Count;
            if (count > maxCount)
            {
                maxCount = count;
                leaderId = player.Id;
            }
        }

        bool changed = false;
        foreach (var player in _game.Players)
        {
            bool shouldHave = player.Id == leaderId && maxCount > 0;
            if (player.VictoryPoints.HasLargestArmy != shouldHave)
            {
                player.VictoryPoints.SetLargestArmy(shouldHave);
                changed = true;
            }
        }

        if (changed)
            _game.AddEvent(new LargestArmyUpdated(leaderId, maxCount));
    }

    /// <summary>US-SC4: check win condition; emits GameEnded if a player has 10 VP.</summary>
    public bool CheckWinCondition()
    {
        // Update bonuses before final VP check
        UpdateLongestRoad();
        UpdateLargestArmy();

        foreach (var player in _game.Players)
        {
            if (player.VictoryPoints.Total >= 10)
            {
                _game.SetGamePhase(GamePhase.Ended);
                _game.AddEvent(new GameEnded(player.Id, player.VictoryPoints.Total));
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// DFS/backtracking to compute the longest continuous road for a player.
    /// An opponent's settlement or city on an intersection breaks the chain (US-SC2 rule).
    /// </summary>
    public int ComputeLongestRoad(GameFlow.Player player)
    {
        var roads = player.Roads.ToList();
        if (roads.Count == 0) return 0;

        // Build adjacency: for each road, which other roads share an endpoint?
        var roadEndpoints = roads
            .Select(r => (road: r, endpoints: r.Location.GetEndpoints().ToList()))
            .ToList();

        // Gather all opponent intersections that break road chains
        var opponentIntersections = _game.Players
            .Where(p => p.Id != player.Id)
            .SelectMany(p => p.Settlements.Select(s => s.Location)
                .Concat(p.Cities.Select(c => c.Location)))
            .ToHashSet();

        int longest = 0;
        var visited = new HashSet<Road>();

        foreach (var (startRoad, _) in roadEndpoints)
        {
            visited.Clear();
            int length = Dfs(startRoad, null, visited, roadEndpoints, opponentIntersections);
            if (length > longest) longest = length;
        }

        return longest;
    }

    private int Dfs(
        Road current,
        Intersection? entryEndpoint,
        HashSet<Road> visited,
        List<(Road road, List<Intersection> endpoints)> roadEndpoints,
        HashSet<Intersection> opponentIntersections)
    {
        visited.Add(current);

        int best = 1;

        var currentEntry = roadEndpoints.First(x => x.road == current);
        foreach (var endpoint in currentEntry.endpoints)
        {
            // An opponent's structure at this intersection breaks the chain
            if (opponentIntersections.Contains(endpoint)) continue;

            // Don't traverse back the way we came
            if (endpoint.Equals(entryEndpoint)) continue;

            foreach (var (neighbor, _) in roadEndpoints)
            {
                if (visited.Contains(neighbor)) continue;
                var neighborEntry = roadEndpoints.First(x => x.road == neighbor);
                if (!neighborEntry.endpoints.Any(ep => ep.Equals(endpoint))) continue;

                int length = 1 + Dfs(neighbor, endpoint, visited, roadEndpoints, opponentIntersections);
                if (length > best) best = length;
            }
        }

        visited.Remove(current);
        return best;
    }
}
