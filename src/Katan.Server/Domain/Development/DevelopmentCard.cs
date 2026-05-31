namespace Katan.Server.Domain.Development;

public class DevelopmentCard
{
    public DevelopmentCardType Type { get; }

    public DevelopmentCard(DevelopmentCardType type)
    {
        Type = type;
    }
}