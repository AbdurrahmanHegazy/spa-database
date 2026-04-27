using IndustrialMonitoring.Api.Models.Alerts;

namespace IndustrialMonitoring.Api.Services;

public class AlertsService : IAlertsService
{
    public AlertsResponse GetAlertsData()
    {
        return new AlertsResponse
        {
            Summary = new List<AlertSummaryItemDto>
            {
                new() { Title = "Critical Alerts", Value = "1", Subtitle = "Immediate action required" },
                new() { Title = "Warnings", Value = "2", Subtitle = "Need operator review" },
                new() { Title = "Acknowledged", Value = "8", Subtitle = "Already handled" },
                new() { Title = "Events Today", Value = "37", Subtitle = "Recorded operational events" }
            },
            ActiveAlerts = new List<ActiveAlertDto>
            {
                new() { Title = "High Temperature", Source = "Mixer Line 2", Severity = "critical", Time = "2 min ago" },
                new() { Title = "Pressure Drop", Source = "Boiler Circuit", Severity = "warning", Time = "5 min ago" },
                new() { Title = "Communication Delay", Source = "Pump Group M030", Severity = "warning", Time = "9 min ago" }
            },
            EventHistory = new List<EventHistoryDto>
            {
                new() { Title = "Motor Restarted", Source = "Pump Group M030", Timestamp = "2026-04-15 10:18:00", Type = "info" },
                new() { Title = "Alarm Acknowledged", Source = "Mixer Line 2", Timestamp = "2026-04-15 10:10:00", Type = "success" },
                new() { Title = "Threshold Exceeded", Source = "Boiler Circuit", Timestamp = "2026-04-15 09:58:00", Type = "warning" },
                new() { Title = "Tag Stream Recovered", Source = "Conveyor Line 1", Timestamp = "2026-04-15 09:42:00", Type = "success" }
            }
        };
    }
}