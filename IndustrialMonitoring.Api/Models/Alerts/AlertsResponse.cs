namespace IndustrialMonitoring.Api.Models.Alerts;

public class AlertsResponse
{
    public List<AlertSummaryItemDto> Summary { get; set; } = new();
    public List<ActiveAlertDto> ActiveAlerts { get; set; } = new();
    public List<EventHistoryDto> EventHistory { get; set; } = new();
}
