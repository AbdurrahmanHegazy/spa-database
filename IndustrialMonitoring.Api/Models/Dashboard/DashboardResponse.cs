namespace IndustrialMonitoring.Api.Models.Dashboard;

public class DashboardResponse
{
    public List<KpiItemDto> Kpis { get; set; } = new();
    public List<SystemCardDto> SystemCards { get; set; } = new();
    public List<RecentAlertDto> RecentAlerts { get; set; } = new();
    public List<TopTagDto> TopTags { get; set; } = new();
}