namespace IndustrialMonitoring.Collector.OpcUa;

public class DiscoveredOpcUaProject
{
    public string Name { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string? RootNodeId { get; set; }

    public List<DiscoveredOpcUaNode> Nodes { get; set; } = new();
}