namespace Katan.Server.Domain.Board;

public class Port
{
    public Edge CoastalEdge { get; }
    public PortType PortType { get; }
    public ResourceType? ResourceType { get; }
    public int TradeRatio => PortType == PortType.Generic ? 3 : 2;

    public Port(Edge coastalEdge, PortType portType, ResourceType? resourceType = null)
    {
        if (portType == PortType.Specialized && resourceType is null)
            throw new ArgumentException("Specialized port must have a resource type.");

        CoastalEdge = coastalEdge;
        PortType = portType;
        ResourceType = resourceType;
    }
}