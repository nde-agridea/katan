namespace Katan.Server.Tests.Board;

using Katan.Server.Domain.Board;
using FluentAssertions;

public class TilePositionTests
{
    [Fact]
    public void Neighbours_ReturnsExactlySixPositions()
    {
        var pos = new TilePosition(0, 0);
        pos.Neighbours().Should().HaveCount(6);
    }

    [Fact]
    public void Neighbours_AreSymmetric()
    {
        var pos = new TilePosition(1, 2);
        foreach (var neighbour in pos.Neighbours())
            neighbour.Neighbours().Should().Contain(pos);
    }

    [Fact]
    public void TilePosition_Equality_WorksCorrectly()
    {
        new TilePosition(1, 2).Should().Be(new TilePosition(1, 2));
        new TilePosition(1, 2).Should().NotBe(new TilePosition(2, 1));
    }
}
