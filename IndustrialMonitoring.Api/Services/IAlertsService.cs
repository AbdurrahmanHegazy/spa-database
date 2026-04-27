using IndustrialMonitoring.Api.Models.Alerts;

namespace IndustrialMonitoring.Api.Services;

public interface IAlertsService
{
    AlertsResponse GetAlertsData();
}