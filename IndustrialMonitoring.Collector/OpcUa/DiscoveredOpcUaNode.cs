namespace IndustrialMonitoring.Collector.OpcUa;

public class DiscoveredOpcUaNode
{
    public string NodeId { get; set; } = string.Empty;
    public string? BrowseName { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? NodeClass { get; set; }
    public int Depth { get; set; }

    public string Kind { get; set; } = "unknown";
    public string? DataType { get; set; }

    public bool IsSelectable { get; set; }
    public bool IsEnabled { get; set; }

    public List<DiscoveredOpcUaNode> Children { get; set; } = new();
}