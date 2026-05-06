namespace IndustrialMonitoring.Api.Models.OpcUa;

public class OpcUaSectionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int ChildCount { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime DiscoveredAtUtc { get; set; }
}