namespace IndustrialMonitoring.Collector.OpcUa;

public class DiscoveredOpcUaSection
{
    public string Name { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int ChildCount { get; set; }
    public List<DiscoveredOpcUaTag> Tags { get; set; } = new();
}

public class DiscoveredOpcUaTag
{
    public string TagName { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DataType { get; set; }
}