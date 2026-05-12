namespace IndustrialMonitoring.Api.Models.OpcUa;

public class DiscoveredTagSyncResultDto
{
    public int ImportedCount { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public List<string> ImportedTags { get; set; } = new();
}