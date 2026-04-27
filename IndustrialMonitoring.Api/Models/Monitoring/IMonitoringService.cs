using IndustrialMonitoring.Api.Models.Monitoring;

namespace IndustrialMonitoring.Api.Services;

public interface IMonitoringService
{
    MonitoringResponse GetMonitoringData();
}