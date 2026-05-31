namespace Katan.Server.Domain.Scoring;

public class VictoryPointLedger
{
    private int _fromStructures;
    private int _fromDevelopmentCards;

    public bool HasLongestRoad { get; private set; }
    public bool HasLargestArmy { get; private set; }

    public int Total => _fromStructures + _fromDevelopmentCards
        + (HasLongestRoad ? 2 : 0)
        + (HasLargestArmy ? 2 : 0);

    public void SetFromStructures(int points) => _fromStructures = points;

    public void AddDevelopmentCardPoint() => _fromDevelopmentCards++;

    public void SetLongestRoad(bool value) => HasLongestRoad = value;

    public void SetLargestArmy(bool value) => HasLargestArmy = value;
}