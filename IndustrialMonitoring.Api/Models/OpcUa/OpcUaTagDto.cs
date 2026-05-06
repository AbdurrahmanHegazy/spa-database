namespace IndustrialMonitoring.Api.Models.OpcUa;

public class OpcUaTagDto
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DataType { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime DiscoveredAtUtc { get; set; }
}