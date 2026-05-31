namespace Katan.Server.Tests.GameFlow;

using Katan.Server.Domain.GameFlow;
using FluentAssertions;

public class MapVotingSessionTests
{
    [Fact]
    public void AllPlayersApprove_MapIsApproved()
    {
        var session = new MapVotingSession(3, generationAttempt: 1);
        session.CastVote("p1", true);
        session.CastVote("p2", true);
        session.CastVote("p3", true);
        session.IsComplete.Should().BeTrue();
        session.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void MajorityReject_MapIsRejected()
    {
        var session = new MapVotingSession(4, generationAttempt: 1);
        session.CastVote("p1", false);
        session.CastVote("p2", false);
        session.CastVote("p3", false);
        session.CastVote("p4", true);
        session.IsComplete.Should().BeTrue();
        session.IsApproved.Should().BeFalse();
        session.NeedsRegeneration.Should().BeTrue();
    }

    [Fact]
    public void FourthGeneration_AlwaysApproved_EvenWithMajorityReject()
    {
        var session = new MapVotingSession(3, generationAttempt: 4);
        session.CastVote("p1", false);
        session.CastVote("p2", false);
        session.CastVote("p3", false);
        session.IsApproved.Should().BeTrue();
        session.ForceAccepted.Should().BeTrue();
    }

    [Fact]
    public void VotingNotComplete_UntilAllPlayersVote()
    {
        var session = new MapVotingSession(3, generationAttempt: 1);
        session.CastVote("p1", true);
        session.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void CastVote_AfterComplete_Throws()
    {
        var session = new MapVotingSession(1, generationAttempt: 1);
        session.CastVote("p1", true);
        var act = () => session.CastVote("p1", false);
        act.Should().Throw<InvalidOperationException>();
    }
}
