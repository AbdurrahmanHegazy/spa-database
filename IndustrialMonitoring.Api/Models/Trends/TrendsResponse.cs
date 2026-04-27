namespace IndustrialMonitoring.Api.Models.Trends;

public class TrendsResponse
{
    public TrendFiltersDto Filters { get; set; } = new();
    public List<TrendStatItemDto> Stats { get; set; } = new();
    public List<TrendSampleRowDto> SampleRows { get; set; } = new();
    public List<TrendPointDto> ChartPoints { get; set; } = new();
}