namespace IndustrialMonitoring.Api.Models.Alerts;

public class ActiveAlertDto
{
    public string Title { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
}