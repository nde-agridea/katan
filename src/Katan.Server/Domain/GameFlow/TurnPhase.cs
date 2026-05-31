namespace Katan.Server.Domain.GameFlow;

public enum TurnPhase
{
    WaitingForRoll,
    ResourceProduction,
    Discard,
    RobberMovement,
    RobberTribute,
    Trade,
    Build,
    End
}