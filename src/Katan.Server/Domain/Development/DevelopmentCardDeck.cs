namespace Katan.Server.Domain.Development;

public class DevelopmentCardDeck
{
    private static readonly IReadOnlyList<DevelopmentCardType> FullDeck = BuildFullDeck();

    private List<DevelopmentCard> _deck;
    private readonly Random _random;

    public DevelopmentCardDeck(Random? random = null)
    {
        _random = random ?? Random.Shared;
        _deck = Shuffle(FullDeck.Select(type => new DevelopmentCard(type)).ToList());
    }

    public int Count => _deck.Count;

    public DevelopmentCard Draw()
    {
        if (_deck.Count == 0)
            _deck = Shuffle(FullDeck.Select(type => new DevelopmentCard(type)).ToList());

        var card = _deck[^1];
        _deck.RemoveAt(_deck.Count - 1);
        return card;
    }

    private List<DevelopmentCard> Shuffle(List<DevelopmentCard> cards)
    {
        for (var i = cards.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }

        return cards;
    }

    private static List<DevelopmentCardType> BuildFullDeck()
    {
        var deck = new List<DevelopmentCardType>();
        for (var i = 0; i < 11; i++) deck.Add(DevelopmentCardType.Knight);
        for (var i = 0; i < 5; i++) deck.Add(DevelopmentCardType.VictoryPoint);
        for (var i = 0; i < 3; i++) deck.Add(DevelopmentCardType.RoadBuilding);
        for (var i = 0; i < 2; i++) deck.Add(DevelopmentCardType.Monopoly);
        for (var i = 0; i < 3; i++) deck.Add(DevelopmentCardType.Excess);
        deck.Add(DevelopmentCardType.Disaster);
        return deck;
    }
}