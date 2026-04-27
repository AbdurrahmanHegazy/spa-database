using IndustrialMonitoring.Api.Models.Dashboard;

namespace IndustrialMonitoring.Api.Services;

public interface IDashboardService
{
    DashboardResponse GetDashboardSummary();
}