namespace Katan.Server.Tests.Resources;

using Katan.Server.Domain.Board;
using Katan.Server.Domain.Resources;
using FluentAssertions;

public class ResourceHandTests
{
    [Fact]
    public void NewHand_HasZeroResourcesOfEachType()
    {
        var hand = new ResourceHand();
        foreach (ResourceType type in Enum.GetValues<ResourceType>())
            hand.Count(type).Should().Be(0);
        hand.Total.Should().Be(0);
    }

    [Fact]
    public void Add_IncreasesCount()
    {
        var hand = new ResourceHand();
        hand.Add(ResourceType.Wood, 3);
        hand.Count(ResourceType.Wood).Should().Be(3);
        hand.Total.Should().Be(3);
    }

    [Fact]
    public void CanAfford_ReturnsTrueWhenSufficient()
    {
        var hand = new ResourceHand();
        hand.Add(ResourceType.Wood, 2);
        hand.Add(ResourceType.Brick, 1);
        var cost = new Dictionary<ResourceType, int> { [ResourceType.Wood] = 2, [ResourceType.Brick] = 1 };
        hand.CanAfford(cost).Should().BeTrue();
    }

    [Fact]
    public void CanAfford_ReturnsFalseWhenInsufficient()
    {
        var hand = new ResourceHand();
        hand.Add(ResourceType.Wood, 1);
        var cost = new Dictionary<ResourceType, int> { [ResourceType.Wood] = 2 };
        hand.CanAfford(cost).Should().BeFalse();
    }

    [Fact]
    public void Pay_DeductsResources()
    {
        var hand = new ResourceHand();
        hand.Add(ResourceType.Wood, 3);
        var cost = new Dictionary<ResourceType, int> { [ResourceType.Wood] = 2 };
        hand.Pay(cost);
        hand.Count(ResourceType.Wood).Should().Be(1);
    }

    [Fact]
    public void Pay_ThrowsWhenInsufficient()
    {
        var hand = new ResourceHand();
        var cost = new Dictionary<ResourceType, int> { [ResourceType.Wood] = 1 };
        var act = () => hand.Pay(cost);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Remove_ThrowsWhenInsufficient()
    {
        var hand = new ResourceHand();
        var act = () => hand.Remove(ResourceType.Wood, 1);
        act.Should().Throw<InvalidOperationException>();
    }
}
