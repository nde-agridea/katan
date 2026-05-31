namespace Katan.Server.Domain.GameFlow;

using Katan.Server.Domain.Development;
using Katan.Server.Domain.Military;
using Katan.Server.Domain.Resources;
using Katan.Server.Domain.Scoring;
using Katan.Server.Domain.Structures;

public class Player
{
    private readonly List<Settlement> _settlements = new();
    private readonly List<City> _cities = new();
    private readonly List<Road> _roads = new();
    private readonly List<Army> _armies = new();

    public string Id { get; }
    public string Name { get; }
    public ResourceHand Hand { get; } = new();
    public VictoryPointLedger VictoryPoints { get; } = new();
    public int RemainingSettlements { get; private set; } = 5;
    public int RemainingCities { get; private set; } = 4;
    public int RemainingRoads { get; private set; } = 15;
    public int RemainingArmies { get; private set; } = 10;
    public IReadOnlyList<Settlement> Settlements => _settlements;
    public IReadOnlyList<City> Cities => _cities;
    public IReadOnlyList<Road> Roads => _roads;
    public IReadOnlyList<Army> Armies => _armies;

    public Player(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public void PlaceSettlement(Settlement settlement)
    {
        if (RemainingSettlements == 0)
            throw new InvalidOperationException("No settlements remaining.");

        _settlements.Add(settlement);
        RemainingSettlements--;
        VictoryPoints.SetFromStructures(ComputeStructureVp());
    }

    public void PlaceCity(City city, Settlement replaced)
    {
        if (RemainingCities == 0)
            throw new InvalidOperationException("No cities remaining.");

        _settlements.Remove(replaced);
        RemainingSettlements++;
        _cities.Add(city);
        RemainingCities--;
        VictoryPoints.SetFromStructures(ComputeStructureVp());
    }

    /// <summary>Place a city directly without replacing an existing settlement (e.g., transferred via combat).</summary>
    public void PlaceCityDirect(City city)
    {
        if (RemainingCities == 0)
            throw new InvalidOperationException("No cities remaining.");

        _cities.Add(city);
        RemainingCities--;
        VictoryPoints.SetFromStructures(ComputeStructureVp());
    }

    /// <summary>Transfer a conquered settlement into this player's collection without consuming a build piece.</summary>
    public void TransferSettlementIn(Settlement settlement)
    {
        _settlements.Add(settlement);
        VictoryPoints.SetFromStructures(ComputeStructureVp());
    }

    /// <summary>Transfer a conquered city into this player's collection without consuming a build piece.</summary>
    public void TransferCityIn(City city)
    {
        _cities.Add(city);
        VictoryPoints.SetFromStructures(ComputeStructureVp());
    }

    public void PlaceRoad(Road road)
    {
        if (RemainingRoads == 0)
            throw new InvalidOperationException("No roads remaining.");

        _roads.Add(road);
        RemainingRoads--;
    }

    public void PlaceArmy(Army army)
    {
        if (RemainingArmies == 0)
            throw new InvalidOperationException("No armies remaining.");

        _armies.Add(army);
        RemainingArmies--;
    }

    public void ReturnArmyToSupply(Army army)
    {
        _armies.Remove(army);
        RemainingArmies++;
    }

    public void RemoveSettlement(Settlement settlement)
    {
        _settlements.Remove(settlement);
        RemainingSettlements++;
        VictoryPoints.SetFromStructures(ComputeStructureVp());
    }

    public void RemoveCity(City city)
    {
        _cities.Remove(city);
        RemainingCities++;
        VictoryPoints.SetFromStructures(ComputeStructureVp());
    }

    private int ComputeStructureVp() => _settlements.Count + (_cities.Count * 2);
}