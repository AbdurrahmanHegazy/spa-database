using System.Globalization;
using IndustrialMonitoring.Api.Helpers;
using IndustrialMonitoring.Api.Models.Trends;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace IndustrialMonitoring.Api.Services;

public class TrendsService : ITrendsService
{
    private readonly string _connectionString;

    public TrendsService(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
    }

    private static string ResolveSelectedTag(string? tagId)
    {
        if (string.IsNullOrWhiteSpace(tagId))
        {
            return string.Empty;
        }

        return tagId.Trim();
    }

    public TrendsResponse GetTrendsData(string? tagId, string? from, string? to, string? timeRange)
    {
        var selectedTag = ResolveSelectedTag(tagId);

        var (resolvedFrom, resolvedTo, resolvedTimeRange) = ResolveTimeWindow(from, to, timeRange);

        if (string.IsNullOrWhiteSpace(selectedTag))
        {
            return new TrendsResponse
            {
                Filters = new TrendFiltersDto
                {
                    SelectedTag = string.Empty,
                    TimeRange = resolvedTimeRange,
                    From = TimeZoneHelper.UtcToRome(resolvedFrom).ToString("yyyy-MM-dd HH:mm"),
                    To = TimeZoneHelper.UtcToRome(resolvedTo).ToString("yyyy-MM-dd HH:mm")
                },
                Stats = new List<TrendStatItemDto>
                {
                    new() { Title = "Minimum", Value = "0", Subtitle = "value" },
                    new() { Title = "Maximum", Value = "0", Subtitle = "value" },
                    new() { Title = "Average", Value = "0", Subtitle = "value" },
                    new() { Title = "Samples", Value = "0", Subtitle = "points" }
                },
                SampleRows = new List<TrendSampleRowDto>(),
                ChartPoints = new List<TrendPointDto>()
            };
        }

        var rows = LoadTrendRows(selectedTag, resolvedFrom, resolvedTo);

        var numericRows = rows
            .Where(x => x.NumericValue.HasValue)
            .ToList();

        var min = numericRows.Count > 0 ? numericRows.Min(x => x.NumericValue!.Value) : 0;
        var max = numericRows.Count > 0 ? numericRows.Max(x => x.NumericValue!.Value) : 0;
        var avg = numericRows.Count > 0 ? numericRows.Average(x => x.NumericValue!.Value) : 0;

        return new TrendsResponse
        {
            Filters = new TrendFiltersDto
            {
                SelectedTag = selectedTag,
                TimeRange = resolvedTimeRange,
                From = TimeZoneHelper.UtcToRome(resolvedFrom).ToString("yyyy-MM-dd HH:mm"),
                To = TimeZoneHelper.UtcToRome(resolvedTo).ToString("yyyy-MM-dd HH:mm")
            },
            Stats = new List<TrendStatItemDto>
            {
                new()
                {
                    Title = "Minimum",
                    Value = min.ToString("0.##", CultureInfo.InvariantCulture),
                    Subtitle = "value"
                },
                new()
                {
                    Title = "Maximum",
                    Value = max.ToString("0.##", CultureInfo.InvariantCulture),
                    Subtitle = "value"
                },
                new()
                {
                    Title = "Average",
                    Value = avg.ToString("0.##", CultureInfo.InvariantCulture),
                    Subtitle = "value"
                },
                new()
                {
                    Title = "Samples",
                    Value = numericRows.Count.ToString("N0", CultureInfo.InvariantCulture),
                    Subtitle = "points"
                }
            },
            SampleRows = numericRows
                .TakeLast(10)
                .Select(x => new TrendSampleRowDto
                {
                    Time = TimeZoneHelper.UtcToRome(x.Timestamp).ToString("HH:mm"),
                    Value = x.NumericValue!.Value.ToString("0.##", CultureInfo.InvariantCulture)
                })
                .ToList(),
            ChartPoints = numericRows
                .Select(x => new TrendPointDto
                {
                    Time = TimeZoneHelper.UtcToRome(x.Timestamp).ToString("HH:mm"),
                    Value = x.NumericValue!.Value
                })
                .ToList()
        };
    }

    private List<TrendRow> LoadTrendRows(string tagName, DateTime fromUtc, DateTime toUtc)
    {
        var results = new List<TrendRow>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
            SELECT tag_name, value, "timestamp", quality, source
            FROM public.tag_readings
            WHERE tag_name = @tagName
              AND "timestamp" >= @fromUtc
              AND "timestamp" <= @toUtc
            ORDER BY "timestamp" ASC
            """;

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tagName", tagName);
        command.Parameters.AddWithValue("fromUtc", fromUtc);
        command.Parameters.AddWithValue("toUtc", toUtc);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var rawValue = reader["value"]?.ToString() ?? string.Empty;

            double? numericValue = null;
            if (double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                numericValue = parsed;
            }

            results.Add(new TrendRow
            {
                TagName = reader["tag_name"]?.ToString() ?? string.Empty,
                RawValue = rawValue,
                NumericValue = numericValue,
                Timestamp = DateTime.SpecifyKind(
                    reader.GetDateTime(reader.GetOrdinal("timestamp")),
                    DateTimeKind.Utc),
                Quality = reader["quality"]?.ToString() ?? string.Empty,
                Source = reader["source"]?.ToString() ?? string.Empty
            });
        }

        return results;
    }

    private static (DateTime fromUtc, DateTime toUtc, string resolvedTimeRange) ResolveTimeWindow(
        string? from,
        string? to,
        string? timeRange)
    {
        var nowUtc = DateTime.UtcNow;

        if (DateTime.TryParse(from, out var parsedFrom) && DateTime.TryParse(to, out var parsedTo))
        {
            return (
                DateTime.SpecifyKind(parsedFrom, DateTimeKind.Local).ToUniversalTime(),
                DateTime.SpecifyKind(parsedTo, DateTimeKind.Local).ToUniversalTime(),
                string.IsNullOrWhiteSpace(timeRange) ? "custom" : timeRange
            );
        }

        return (timeRange ?? "last-1h") switch
        {
            "last-15m" => (nowUtc.AddMinutes(-15), nowUtc, "last-15m"),
            "last-6h" => (nowUtc.AddHours(-6), nowUtc, "last-6h"),
            "last-24h" => (nowUtc.AddHours(-24), nowUtc, "last-24h"),
            "custom" => (nowUtc.AddHours(-1), nowUtc, "custom"),
            _ => (nowUtc.AddHours(-1), nowUtc, "last-1h")
        };
    }

    private class TrendRow
    {
        public string TagName { get; set; } = string.Empty;
        public string RawValue { get; set; } = string.Empty;
        public double? NumericValue { get; set; }
        public DateTime Timestamp { get; set; }
        public string Quality { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}