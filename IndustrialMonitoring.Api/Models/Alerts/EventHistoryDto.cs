namespace IndustrialMonitoring.Api.Models.Alerts;

public class EventHistoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}