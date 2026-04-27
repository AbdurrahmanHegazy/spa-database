using IndustrialMonitoring.Api.Models.Monitoring;

namespace IndustrialMonitoring.Api.Services;

public class MonitoringService : IMonitoringService
{
    public MonitoringResponse GetMonitoringData()
    {
        return new MonitoringResponse
        {
            Summary = new List<MonitoringSummaryItemDto>
            {
                new() { Title = "Streaming Tags", Value = "128", Subtitle = "Currently active live points" },
                new() { Title = "Healthy Signals", Value = "121", Subtitle = "Good quality values" },
                new() { Title = "Warning Signals", Value = "5", Subtitle = "Need attention" },
                new() { Title = "Offline Devices", Value = "2", Subtitle = "No recent updates" }
            },
            LiveTags = new List<LiveTagDto>
            {
                new()
                {
                    Id = "motor-m030-frequency",
                    Group = "Pump Group M030",
                    Tag = "Motori_BM_Pompa_i_M030.frequenza attuale",
                    Value = "123.45 Hz",
                    Quality = "Good",
                    Freshness = "2s ago",
                    Status = "online"
                },
                new()
                {
                    Id = "motor-m030-current",
                    Group = "Pump Group M030",
                    Tag = "Motori_BM_Pompa_i_M030.corrente attuale",
                    Value = "17.80 A",
                    Quality = "Good",
                    Freshness = "3s ago",
                    Status = "online"
                },
                new()
                {
                    Id = "boiler-main-pressure",
                    Group = "Boiler Circuit",
                    Tag = "Boiler_Main.Pressure",
                    Value = "7.80 bar",
                    Quality = "Warning",
                    Freshness = "5s ago",
                    Status = "warning"
                },
                new()
                {
                    Id = "mixer-line-2-temperature",
                    Group = "Mixer Line 2",
                    Tag = "Mixer_Line_2.Temperature",
                    Value = "88.20 °C",
                    Quality = "Good",
                    Freshness = "2s ago",
                    Status = "online"
                },
                new()
                {
                    Id = "conveyor-line-1-speed",
                    Group = "Conveyor Line 1",
                    Tag = "Line_1.Conveyor.Speed",
                    Value = "65.20 rpm",
                    Quality = "Good",
                    Freshness = "1s ago",
                    Status = "online"
                },
                new()
                {
                    Id = "motor-m800-status",
                    Group = "Pump Group M800",
                    Tag = "Motori_CO_Cond_M800.stato attuale",
                    Value = "Unavailable",
                    Quality = "Bad",
                    Freshness = "45s ago",
                    Status = "offline"
                }
            }
        };
    }
}