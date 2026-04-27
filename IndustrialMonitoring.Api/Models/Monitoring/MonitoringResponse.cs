namespace IndustrialMonitoring.Api.Models.Monitoring;

public class MonitoringResponse
{
    public List<MonitoringSummaryItemDto> Summary { get; set; } = new();
    public List<LiveTagDto> LiveTags { get; set; } = new();
}