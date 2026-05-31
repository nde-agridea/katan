namespace Katan.Server.Domain.Board;

public class MapGenerator
{
    private readonly Random _random;

    public MapGenerator(Random? random = null)
    {
        _random = random ?? Random.Shared;
    }

    public Board GenerateMap()
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var positions = TryGenerateShape();
            if (positions is null)
                continue;

            var (tileTypes, desert) = AssignTileTypes(positions);
            if (!ValidateDesertPlacement(positions, desert))
                continue;

            var tokens = AssignNumberTokens(positions, desert);
            if (tokens is null)
                continue;

            var ports = GeneratePorts(positions);
            if (ports is null)
                continue;

            var board = new Board();
            foreach (var position in positions)
            {
                var tile = new Tile(position, tileTypes[position], tokens[position]);
                board.AddTile(tile);
            }

            foreach (var port in ports)
                board.AddPort(port);

            board.PlaceRobber(desert);
            return board;
        }

        throw new InvalidOperationException("Could not generate a valid map after 100 attempts.");
    }

    private List<TilePosition>? TryGenerateShape()
    {
        var placed = new HashSet<TilePosition>();
        var frontier = new List<TilePosition>();
        var start = new TilePosition(0, 0);
        placed.Add(start);
        frontier.AddRange(start.Neighbours());

        while (placed.Count < 19 && frontier.Count > 0)
        {
            var index = _random.Next(frontier.Count);
            var candidate = frontier[index];
            frontier.RemoveAt(index);
            if (placed.Contains(candidate))
                continue;

            placed.Add(candidate);
            foreach (var neighbour in candidate.Neighbours())
            {
                if (!placed.Contains(neighbour) && !frontier.Contains(neighbour))
                    frontier.Add(neighbour);
            }
        }

        if (placed.Count < 19)
            return null;

        var positions = placed.ToList();
        return ValidateShape(positions) ? positions : null;
    }

    private bool ValidateShape(List<TilePosition> positions)
    {
        var positionSet = new HashSet<TilePosition>(positions);
        var minQ = positions.Min(p => p.Q);
        var maxQ = positions.Max(p => p.Q);
        var minR = positions.Min(p => p.R);
        var maxR = positions.Max(p => p.R);

        for (var q = minQ - 1; q <= maxQ + 1; q++)
        {
            for (var r = minR - 1; r <= maxR + 1; r++)
            {
                var position = new TilePosition(q, r);
                if (positionSet.Contains(position))
                    continue;

                var neighbourCount = position.Neighbours().Count(positionSet.Contains);
                if (neighbourCount == 6)
                    return false;
            }
        }

        foreach (var tile in positions)
        {
            var neighboursInMap = tile.Neighbours().Count(positionSet.Contains);
            if (neighboursInMap >= 2)
                continue;

            var chainLength = CountPeninsulaChain(tile, positionSet);
            if (chainLength > 2)
                return false;
        }

        return true;
    }

    private static int CountPeninsulaChain(TilePosition start, HashSet<TilePosition> positionSet)
    {
        var visited = new HashSet<TilePosition>();
        var queue = new Queue<TilePosition>();
        queue.Enqueue(start);
        visited.Add(start);
        var count = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            count++;
            foreach (var neighbour in current.Neighbours())
            {
                if (!positionSet.Contains(neighbour) || visited.Contains(neighbour))
                    continue;

                var neighbourCount = neighbour.Neighbours().Count(positionSet.Contains);
                if (neighbourCount <= 2)
                {
                    visited.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }
        }

        return count;
    }

    private (Dictionary<TilePosition, TileType> types, TilePosition desert) AssignTileTypes(List<TilePosition> positions)
    {
        var typePool = new List<TileType>();
        for (var i = 0; i < 4; i++) typePool.Add(TileType.Forest);
        for (var i = 0; i < 3; i++) typePool.Add(TileType.Quarry);
        for (var i = 0; i < 3; i++) typePool.Add(TileType.Field);
        for (var i = 0; i < 3; i++) typePool.Add(TileType.Pasture);
        for (var i = 0; i < 3; i++) typePool.Add(TileType.Mountain);
        for (var i = 0; i < 2; i++) typePool.Add(TileType.Mine);
        typePool.Add(TileType.Desert);

        Shuffle(typePool);

        var positionSet = new HashSet<TilePosition>(positions);
        var result = new Dictionary<TilePosition, TileType>();
        TilePosition desert = default!;

        for (var attempt = 0; attempt < 200; attempt++)
        {
            Shuffle(typePool);
            result.Clear();
            for (var i = 0; i < positions.Count; i++)
                result[positions[i]] = typePool[i];

            var desertPosition = result.First(kv => kv.Value == TileType.Desert).Key;
            if (desertPosition.Neighbours().Count(positionSet.Contains) < 6)
                continue;

            if (!ValidateResourceDistribution(result, positionSet))
                continue;

            desert = desertPosition;
            return (result, desert);
        }

        result.Clear();
        for (var i = 0; i < positions.Count; i++)
            result[positions[i]] = typePool[i];

        desert = result.First(kv => kv.Value == TileType.Desert).Key;
        return (result, desert);
    }

    private static bool ValidateDesertPlacement(List<TilePosition> positions, TilePosition desert)
    {
        var positionSet = new HashSet<TilePosition>(positions);
        return desert.Neighbours().Count(positionSet.Contains) == 6;
    }

    private static bool ValidateResourceDistribution(Dictionary<TilePosition, TileType> types, HashSet<TilePosition> positionSet)
    {
        foreach (var (position, type) in types)
        {
            if (type == TileType.Desert)
                continue;

            var neighbours = position.Neighbours().Where(positionSet.Contains).ToList();
            for (var i = 0; i < neighbours.Count - 1; i++)
            {
                for (var j = i + 1; j < neighbours.Count; j++)
                {
                    if (types[neighbours[i]] == type && types[neighbours[j]] == type && neighbours[i].Neighbours().Contains(neighbours[j]))
                        return false;
                }
            }
        }

        return true;
    }

    private Dictionary<TilePosition, int>? AssignNumberTokens(List<TilePosition> positions, TilePosition desert)
    {
        var positionSet = new HashSet<TilePosition>(positions);
        var tokenPool = new List<int> { 2, 3, 3, 4, 4, 5, 5, 6, 6, 8, 8, 9, 9, 10, 10, 11, 11, 12 };
        var nonDesertPositions = positions.Where(position => position != desert).ToList();

        for (var attempt = 0; attempt < 200; attempt++)
        {
            var tokens = tokenPool.ToList();
            Shuffle(tokens);

            var assignment = new Dictionary<TilePosition, int> { [desert] = 7 };
            for (var i = 0; i < nonDesertPositions.Count; i++)
                assignment[nonDesertPositions[i]] = tokens[i];

            if (ValidateTokenAssignment(assignment, positionSet))
                return assignment;
        }

        return null;
    }

    private static bool ValidateTokenAssignment(Dictionary<TilePosition, int> tokens, HashSet<TilePosition> positionSet)
    {
        var highValueTokens = new HashSet<int> { 5, 6, 8, 9 };
        foreach (var (position, token) in tokens)
        {
            if (!highValueTokens.Contains(token))
                continue;

            foreach (var neighbour in position.Neighbours())
            {
                if (positionSet.Contains(neighbour) && highValueTokens.Contains(tokens[neighbour]))
                    return false;
            }
        }

        return true;
    }

    private List<Port>? GeneratePorts(List<TilePosition> positions)
    {
        var positionSet = new HashSet<TilePosition>(positions);
        var coastalTiles = positions.Where(position => position.Neighbours().Any(neighbour => !positionSet.Contains(neighbour))).ToList();
        var coastalEdges = new List<Edge>();

        foreach (var tile in coastalTiles)
        {
            foreach (var neighbour in tile.Neighbours())
            {
                if (!positionSet.Contains(neighbour))
                    coastalEdges.Add(new Edge(tile, neighbour));
            }
        }

        if (coastalEdges.Count < 8)
            return null;

        var portTypes = new List<(PortType type, ResourceType? resource)>
        {
            (PortType.Specialized, ResourceType.Wood),
            (PortType.Specialized, ResourceType.Brick),
            (PortType.Specialized, ResourceType.Wheat),
            (PortType.Specialized, ResourceType.Sheep),
            (PortType.Specialized, ResourceType.Stone),
            (PortType.Specialized, ResourceType.Iron),
            (PortType.Generic, null),
            (PortType.Generic, null)
        };

        for (var attempt = 0; attempt < 100; attempt++)
        {
            var shuffledEdges = coastalEdges.ToList();
            Shuffle(shuffledEdges);

            var selectedEdges = new List<Edge>();
            foreach (var edge in shuffledEdges)
            {
                if (selectedEdges.Count >= 8)
                    break;

                if (!IsAdjacentToAnySelectedPort(edge, selectedEdges))
                    selectedEdges.Add(edge);
            }

            if (selectedEdges.Count < 8)
                continue;

            var shuffledTypes = portTypes.ToList();
            Shuffle(shuffledTypes);

            var ports = new List<Port>();
            for (var i = 0; i < 8; i++)
                ports.Add(new Port(selectedEdges[i], shuffledTypes[i].type, shuffledTypes[i].resource));

            return ports;
        }

        return null;
    }

    private static bool IsAdjacentToAnySelectedPort(Edge edge, List<Edge> selectedEdges)
    {
        foreach (var selected in selectedEdges)
        {
            if (edge.AdjacentTiles.Overlaps(selected.AdjacentTiles))
                return true;
        }

        return false;
    }

    private void Shuffle<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}