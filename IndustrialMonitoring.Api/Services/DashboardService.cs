using IndustrialMonitoring.Api.Models.Dashboard;

namespace IndustrialMonitoring.Api.Services;

public class DashboardService : IDashboardService
{
    public DashboardResponse GetDashboardSummary()
    {
        return new DashboardResponse
        {
            Kpis = new List<KpiItemDto>
            {
                new() { Title = "Connected Devices", Value = "24", Subtitle = "Active monitored assets" },
                new() { Title = "Live Tags", Value = "128", Subtitle = "Streaming in real time" },
                new() { Title = "Active Alerts", Value = "3", Subtitle = "Require operator attention" },
                new() { Title = "Data Flow", Value = "OK", Subtitle = "MQTT and storage healthy" }
            },
            SystemCards = new List<SystemCardDto>
            {
                new() { Label = "Collector Service", Status = "Online" },
                new() { Label = "MQTT Broker", Status = "Online" },
                new() { Label = "TimescaleDB", Status = "Online" },
                new() { Label = "Redis Cache", Status = "Online" }
            },
            RecentAlerts = new List<RecentAlertDto>
            {
                new() { Title = "High Temperature", Source = "Mixer Line 2" },
                new() { Title = "Pressure Drop", Source = "Boiler Circuit" },
                new() { Title = "Communication Delay", Source = "Pump Group M030" }
            },
            TopTags = new List<TopTagDto>
            {
                new() { Id = "motor-m030", Name = "Motori_BM_Pompa_i_M030.frequenza attuale", Value = "123.45" },
                new() { Id = "boiler-main-pressure", Name = "Boiler_Main.Pressure", Value = "7.80" },
                new() { Id = "conveyor-line-1-speed", Name = "Line_1.Conveyor.Speed", Value = "65.20" }
            }
        };
    }
}