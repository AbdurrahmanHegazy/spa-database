using IndustrialMonitoring.Api.Models.Trends;

namespace IndustrialMonitoring.Api.Services;

public interface ITrendsService
{
    TrendsResponse GetTrendsData(string? tagId, string? from, string? to, string? timeRange);
}