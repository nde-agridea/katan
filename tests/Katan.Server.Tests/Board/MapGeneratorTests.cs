namespace Katan.Server.Tests.Board;

using Katan.Server.Domain.Board;
using FluentAssertions;

public class MapGeneratorTests
{
    [Fact]
    public void GenerateMap_ProducesExactly19Tiles()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        board.Tiles.Should().HaveCount(19);
    }

    [Fact]
    public void GenerateMap_HasExactlyOneDesertTile()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        board.Tiles.Values.Count(t => t.Type == TileType.Desert).Should().Be(1);
    }

    [Fact]
    public void GenerateMap_DesertHasSixNeighbours()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        var desert = board.Tiles.Values.Single(t => t.Type == TileType.Desert);
        board.GetNeighbours(desert.Position).Should().HaveCount(6);
    }

    [Fact]
    public void GenerateMap_DesertTokenIsSeven()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        var desert = board.Tiles.Values.Single(t => t.Type == TileType.Desert);
        desert.NumberToken.Should().Be(7);
    }

    [Fact]
    public void GenerateMap_NoAdjacentHighValueTokens()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        var highValue = new HashSet<int> { 5, 6, 8, 9 };
        foreach (var tile in board.Tiles.Values)
        {
            if (!highValue.Contains(tile.NumberToken)) continue;
            foreach (var neighbour in board.GetNeighbours(tile.Position))
                highValue.Should().NotContain(neighbour.NumberToken,
                    $"tile at {tile.Position} (token {tile.NumberToken}) is adjacent to tile at {neighbour.Position} (token {neighbour.NumberToken})");
        }
    }

    [Fact]
    public void GenerateMap_HasEightPorts()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        board.Ports.Should().HaveCount(8);
    }

    [Fact]
    public void GenerateMap_HasSixSpecializedPortsAndTwoGeneric()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        board.Ports.Count(p => p.PortType == PortType.Specialized).Should().Be(6);
        board.Ports.Count(p => p.PortType == PortType.Generic).Should().Be(2);
    }

    [Fact]
    public void GenerateMap_CorrectResourceTypeCounts()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        board.Tiles.Values.Count(t => t.Type == TileType.Forest).Should().Be(4);
        board.Tiles.Values.Count(t => t.Type == TileType.Quarry).Should().Be(3);
        board.Tiles.Values.Count(t => t.Type == TileType.Field).Should().Be(3);
        board.Tiles.Values.Count(t => t.Type == TileType.Pasture).Should().Be(3);
        board.Tiles.Values.Count(t => t.Type == TileType.Mountain).Should().Be(3);
        board.Tiles.Values.Count(t => t.Type == TileType.Mine).Should().Be(2);
    }

    [Fact]
    public void GenerateMap_RobberStartsOnDesert()
    {
        var generator = new MapGenerator(new Random(42));
        var board = generator.GenerateMap();
        var desert = board.Tiles.Values.Single(t => t.Type == TileType.Desert);
        board.RobberPosition.Should().Be(desert.Position);
    }
}
