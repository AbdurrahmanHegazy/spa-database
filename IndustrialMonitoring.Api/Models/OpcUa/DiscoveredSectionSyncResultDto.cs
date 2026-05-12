namespace IndustrialMonitoring.Api.Models.OpcUa;

public class DiscoveredSectionSyncResultDto
{
    public int ImportedCount { get; set; }
    public List<string> ImportedSections { get; set; } = new();
}