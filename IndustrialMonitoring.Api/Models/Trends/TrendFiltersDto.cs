namespace IndustrialMonitoring.Api.Models.Trends;

public class TrendFiltersDto
{
    public string SelectedTag { get; set; } = string.Empty;
    public string TimeRange { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}