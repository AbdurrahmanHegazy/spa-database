using System.Text.Json;
using IndustrialMonitoring.Api.Models.Monitoring;
using Npgsql;
using StackExchange.Redis;

namespace IndustrialMonitoring.Api.Services;

public class MonitoringService : IMonitoringService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redis;
    private readonly string _connectionString;
    private const string RedisKeyPrefix = "industrial:latest:";

    public MonitoringService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redisConnection = redis;
        _redis = redis.GetDatabase();
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
    }

    public MonitoringResponse GetMonitoringData()
    {
        var enabledNodeIds = GetEnabledTagNodeIdsFromDatabase();
        var latestReadings = GetLatestReadingsFromRedis(enabledNodeIds);

        var healthySignals = latestReadings.Count(x =>
            string.Equals(x.Quality, "Good", StringComparison.OrdinalIgnoreCase));

        var warningSignals = latestReadings.Count(x =>
            !string.Equals(x.Quality, "Good", StringComparison.OrdinalIgnoreCase));

        var offlineDevices = latestReadings.Count(x => GetStatus(x) == "offline");

        return new MonitoringResponse
        {
            Summary = new List<MonitoringSummaryItemDto>
            {
                new()
                {
                    Title = "Streaming Tags",
                    Value = latestReadings.Count.ToString(),
                    Subtitle = "Enabled live points"
                },
                new()
                {
                    Title = "Healthy Signals",
                    Value = healthySignals.ToString(),
                    Subtitle = "Good quality enabled values"
                },
                new()
                {
                    Title = "Warning Signals",
                    Value = warningSignals.ToString(),
                    Subtitle = "Enabled tags needing attention"
                },
                new()
                {
                    Title = "Offline Tags",
                    Value = offlineDevices.ToString(),
                    Subtitle = "Enabled tags with no recent updates"
                }
            },

            LiveTags = latestReadings
                .OrderBy(x => x.TagName)
                .Select(x => new LiveTagDto
                {
                    Id = BuildTagId(x.TagName),
                    Group = ExtractGroupName(x.TagName),
                    Tag = FormatTagDisplayName(x.TagName),
                    Value = x.Value,
                    Quality = x.Quality,
                    Freshness = FormatFreshness(x.Timestamp),
                    Status = GetStatus(x)
                })
                .ToList()
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
            INNER JOIN public.opcua_sections s
                ON s.id = t.section_id
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

        return results;
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

        return "Unknown Group";
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

    private static string FormatFreshness(string timestampText)
    {
        if (!DateTime.TryParse(timestampText, out var timestamp))
        {
            return "--";
        }

        var age = DateTime.UtcNow - timestamp.ToUniversalTime();

        if (age.TotalSeconds < 60)
        {
            return $"{Math.Max(1, (int)age.TotalSeconds)}s ago";
        }

        if (age.TotalMinutes < 60)
        {
            return $"{Math.Max(1, (int)age.TotalMinutes)}m ago";
        }

        return $"{Math.Max(1, (int)age.TotalHours)}h ago";
    }

    private static string GetStatus(TagRedisReading reading)
    {
        if (!string.Equals(reading.Quality, "Good", StringComparison.OrdinalIgnoreCase))
        {
            return "warning";
        }

        if (!DateTime.TryParse(reading.Timestamp, out var timestamp))
        {
            return "offline";
        }

        var age = DateTime.UtcNow - timestamp.ToUniversalTime();

        if (age.TotalSeconds > 30)
        {
            return "offline";
        }

        return "online";
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