using IndustrialMonitoring.Api.Models.Trends;

namespace IndustrialMonitoring.Api.Services;

public class TrendsService : ITrendsService
{
    public TrendsResponse GetTrendsData(string? tagId, string? from, string? to, string? timeRange)
    {
        var selectedTag = string.IsNullOrWhiteSpace(tagId)
            ? "Motori_BM_Pompa_i_M030.frequenza attuale"
            : tagId;

        var selectedFrom = string.IsNullOrWhiteSpace(from)
            ? "2026-04-15 10:00"
            : from;

        var selectedTo = string.IsNullOrWhiteSpace(to)
            ? "2026-04-15 11:00"
            : to;

        var selectedTimeRange = string.IsNullOrWhiteSpace(timeRange)
            ? "Last 1 Hour"
            : timeRange;

        return new TrendsResponse
        {
            Filters = new TrendFiltersDto
            {
                SelectedTag = selectedTag,
                TimeRange = selectedTimeRange,
                From = selectedFrom,
                To = selectedTo
            },
            Stats = new List<TrendStatItemDto>
            {
                new() { Title = "Minimum", Value = "118.20", Subtitle = "Hz" },
                new() { Title = "Maximum", Value = "126.90", Subtitle = "Hz" },
                new() { Title = "Average", Value = "123.45", Subtitle = "Hz" },
                new() { Title = "Samples", Value = "1,248", Subtitle = "points" }
            },
            SampleRows = new List<TrendSampleRowDto>
            {
                new() { Time = "10:00", Value = "122.80 Hz" },
                new() { Time = "10:15", Value = "123.10 Hz" },
                new() { Time = "10:30", Value = "124.00 Hz" },
                new() { Time = "10:45", Value = "123.60 Hz" },
                new() { Time = "11:00", Value = "123.45 Hz" }
            },
            ChartPoints = new List<TrendPointDto>
            {
                new() { Time = "10:00", Value = 122.80 },
                new() { Time = "10:05", Value = 123.00 },
                new() { Time = "10:10", Value = 123.40 },
                new() { Time = "10:15", Value = 123.10 },
                new() { Time = "10:20", Value = 124.20 },
                new() { Time = "10:25", Value = 125.10 },
                new() { Time = "10:30", Value = 124.00 },
                new() { Time = "10:35", Value = 123.70 },
                new() { Time = "10:40", Value = 123.30 },
                new() { Time = "10:45", Value = 123.60 },
                new() { Time = "10:50", Value = 122.90 },
                new() { Time = "10:55", Value = 123.20 },
                new() { Time = "11:00", Value = 123.45 }
            }
        };
    }
}