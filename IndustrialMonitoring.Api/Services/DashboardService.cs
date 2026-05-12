using System.Text.Json;
using IndustrialMonitoring.Api.Models.Dashboard;
using Npgsql;
using StackExchange.Redis;

namespace IndustrialMonitoring.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redis;
    private readonly string _connectionString;
    private const string RedisKeyPrefix = "industrial:latest:";

    public DashboardService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redisConnection = redis;
        _redis = redis.GetDatabase();
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
    }

    public DashboardResponse GetDashboardSummary()
    {
        var enabledNodeIds = GetEnabledTagNodeIdsFromDatabase();
        var latestReadings = GetLatestReadingsFromRedis(enabledNodeIds);

        var liveTagsCount = latestReadings.Count;
        var activeAlertsCount = latestReadings.Count(x =>
            !string.Equals(x.Quality, "Good", StringComparison.OrdinalIgnoreCase));

        var connectedDevicesCount = latestReadings
            .Select(x => ExtractGroupName(x.TagName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        var topTags = latestReadings
            .Take(5)
            .Select(x => new TopTagDto
            {
                Id = BuildTagId(x.TagName),
                Name = FormatTagDisplayName(x.TagName),
                Value = x.Value
            })
            .ToList();

        return new DashboardResponse
        {
            Kpis = new List<KpiItemDto>
            {
                new()
                {
                    Title = "Connected Devices",
                    Value = connectedDevicesCount.ToString(),
                    Subtitle = "Enabled live monitored groups"
                },
                new()
                {
                    Title = "Live Tags",
                    Value = liveTagsCount.ToString(),
                    Subtitle = "Enabled tags streaming in real time"
                },
                new()
                {
                    Title = "Active Alerts",
                    Value = activeAlertsCount.ToString(),
                    Subtitle = "Enabled tags requiring attention"
                },
                new()
                {
                    Title = "Data Flow",
                    Value = liveTagsCount > 0 ? "OK" : "NO DATA",
                    Subtitle = "Enabled live data available"
                }
            },

            RecentAlerts = latestReadings
                .Where(x => !string.Equals(x.Quality, "Good", StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .Select(x => new RecentAlertDto
                {
                    Title = $"Tag quality issue: {FormatTagDisplayName(x.TagName)}",
                    Source = x.Source
                })
                .ToList(),

            TopTags = topTags
        };
    }

    private HashSet<string> GetEnabledTagNodeIdsFromDatabase()
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
            SELECT t.node_id
            FROM public.opcua_tags t
            WHERE t.is_enabled = TRUE;
            """;

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var nodeId = reader.GetString(reader.GetOrdinal("node_id"));
            results.Add(nodeId);
        }

        return results;
    }

    private List<TagRedisReading> GetLatestReadingsFromRedis(HashSet<string> enabledNodeIds)
    {
        var results = new List<TagRedisReading>();

        if (enabledNodeIds.Count == 0)
        {
            return results;
        }

        var endpoints = _redisConnection.GetEndPoints();
        if (endpoints.Length == 0)
        {
            return results;
        }

        var server = _redisConnection.GetServer(endpoints.First());

        foreach (var key in server.Keys(pattern: $"{RedisKeyPrefix}*"))
        {
            var rawValue = _redis.StringGet(key);

            if (rawValue.IsNullOrEmpty)
            {
                continue;
            }

            var json = rawValue.ToString();

            if (json.StartsWith("\""))
            {
                json = JsonSerializer.Deserialize<string>(json) ?? json;
            }

            var reading = JsonSerializer.Deserialize<TagRedisReading>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (reading is null)
            {
                continue;
            }

            if (!enabledNodeIds.Contains(reading.TagName))
            {
                continue;
            }

            results.Add(reading);
        }

        return results
            .OrderBy(x => x.TagName)
            .ToList();
    }

    private static string FormatTagDisplayName(string tagName)
    {
        var quotedParts = GetQuotedParts(tagName);

        if (quotedParts.Count >= 2)
        {
            return $"{quotedParts[1]} - {quotedParts[^1]}";
        }

        if (quotedParts.Count == 1)
        {
            return quotedParts[0];
        }

        return tagName;
    }

    private static string BuildTagId(string tagName)
    {
        return tagName
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("\"", "")
            .Replace(".", "-")
            .Replace(";", "-")
            .Replace("=", "-");
    }

    private static string ExtractGroupName(string tagName)
    {
        var quotedParts = GetQuotedParts(tagName);

        if (quotedParts.Count >= 1)
        {
            return quotedParts[0];
        }

        var dotParts = tagName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (dotParts.Length > 0)
        {
            return dotParts[0];
        }

        return tagName;
    }

    private static List<string> GetQuotedParts(string text)
    {
        var parts = new List<string>();
        var start = -1;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '"')
            {
                if (start == -1)
                {
                    start = i + 1;
                }
                else
                {
                    parts.Add(text[start..i]);
                    start = -1;
                }
            }
        }

        return parts;
    }

    private class TagRedisReading
    {
        public string TagName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Quality { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}