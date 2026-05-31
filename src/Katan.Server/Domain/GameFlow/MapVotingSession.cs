namespace Katan.Server.Domain.GameFlow;

using Katan.Server.Domain.Board.Events;

public class MapVotingSession
{
    private const int MaxRejections = 3;

    private readonly int _totalPlayers;
    private readonly Dictionary<string, bool> _votes = new();
    private int _generationAttempt;

    public bool IsComplete { get; private set; }
    public bool IsApproved { get; private set; }
    public bool ForceAccepted { get; private set; }
    public int GenerationAttempt => _generationAttempt;

    public MapVotingSession(int totalPlayers, int generationAttempt = 1)
    {
        _totalPlayers = totalPlayers;
        _generationAttempt = generationAttempt;
    }

    public void CastVote(string playerId, bool approve)
    {
        if (IsComplete)
            throw new InvalidOperationException("Voting is already complete.");

        _votes[playerId] = approve;
        EvaluateIfReady();
    }

    private void EvaluateIfReady()
    {
        if (_votes.Count < _totalPlayers)
            return;

        var approvals = _votes.Values.Count(vote => vote);
        var rejections = _votes.Values.Count(vote => !vote);

        if (_generationAttempt >= MaxRejections + 1)
        {
            IsComplete = true;
            IsApproved = true;
            ForceAccepted = true;
            return;
        }

        IsComplete = true;
        IsApproved = rejections <= approvals;
    }

    public bool NeedsRegeneration => IsComplete && !IsApproved;

    public int NextGenerationAttempt => _generationAttempt + 1;
}