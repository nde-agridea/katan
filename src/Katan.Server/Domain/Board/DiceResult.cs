namespace Katan.Server.Domain.Board;

public record DiceResult(int Die1, int Die2)
{
    public int Total => Die1 + Die2;

    public bool IsSeven => Total == 7;
}