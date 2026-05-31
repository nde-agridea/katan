namespace Katan.Server.Domain.GameFlow;

using Katan.Server.Domain;
using Katan.Server.Domain.GameFlow.Events;
using Katan.Server.Domain.Resources;

public class Game
{
    private readonly List<Player> _players = new();
    private readonly List<IDomainEvent> _domainEvents = new();
    private int _activePlayerIndex;
    private List<string> _placementOrder = new();

    public Guid Id { get; } = Guid.NewGuid();
    public Board.Board Board { get; private set; } = new();
    public GamePhase Phase { get; private set; } = GamePhase.MapGeneration;
    public TurnPhase TurnPhase { get; private set; } = TurnPhase.WaitingForRoll;
    public IReadOnlyList<Player> Players => _players;
    public Player? ActivePlayer => _players.Count > 0 ? _players[_activePlayerIndex] : null;
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    public MapVotingSession? VotingSession { get; private set; }
    public PendingTrade? PendingTrade { get; private set; }
    public IReadOnlyList<string> PlacementOrder => _placementOrder;
    public int PlacementOrderIndex { get; private set; }

    public void AddPlayer(Player player) => _players.Add(player);

    public void SetBoard(Board.Board board) => Board = board;

    public void InitializeVoting() =>
        VotingSession = new MapVotingSession(_players.Count);

    public void StartNewVotingRound(int generationAttempt) =>
        VotingSession = new MapVotingSession(_players.Count, generationAttempt);

    public void SetPendingTrade(PendingTrade? trade) => PendingTrade = trade;

    public void SetPlacementOrder(List<string> order)
    {
        _placementOrder = order;
        PlacementOrderIndex = 0;
    }

    public void AdvancePlacementOrderIndex() => PlacementOrderIndex++;

    public void StartGame(string firstPlayerId)
    {
        var index = _players.FindIndex(player => player.Id == firstPlayerId);
        if (index < 0)
            throw new InvalidOperationException("Player not found.");

        _activePlayerIndex = index;
        Phase = GamePhase.InitialPlacement;
        AddEvent(new GameStarted(firstPlayerId));
    }

    public void StartTurn()
    {
        Phase = GamePhase.InProgress;
        TurnPhase = TurnPhase.WaitingForRoll;
        AddEvent(new TurnStarted(ActivePlayer!.Id));
    }

    public void SetActivePlayerIndex(int index) => _activePlayerIndex = index;

    public void AdvanceTurnToNextPlayer()
    {
        var previous = ActivePlayer!.Id;
        _activePlayerIndex = (_activePlayerIndex + 1) % _players.Count;
        TurnPhase = TurnPhase.WaitingForRoll;
        AddEvent(new TurnEnded(previous, ActivePlayer!.Id));
    }

    public void SetTurnPhase(TurnPhase phase) => TurnPhase = phase;

    public void SetGamePhase(GamePhase phase) => Phase = phase;

    public void AddEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearEvents() => _domainEvents.Clear();
}