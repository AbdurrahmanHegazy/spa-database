using System.Text.Json;
using IndustrialMonitoring.Api.Models.Dashboard;
using StackExchange.Redis;

namespace IndustrialMonitoring.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redis;
    private const string RedisKeyPrefix = "industrial:latest:";

    public DashboardService(IConnectionMultiplexer redis)
    {
        _redisConnection = redis;
        _redis = redis.GetDatabase();
    }

    public DashboardResponse GetDashboardSummary()
    {
        var latestReadings = GetLatestReadingsFromRedis();

        var liveTagsCount = latestReadings.Count;
        var activeAlertsCount = latestReadings.Count(x => !string.Equals(x.Quality, "Good", StringComparison.OrdinalIgnoreCase));
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
                    Subtitle = "Live monitored assets"
                },
                new()
                {
                    Title = "Live Tags",
                    Value = liveTagsCount.ToString(),
                    Subtitle = "Streaming in real time"
                },
                new()
                {
                    Title = "Active Alerts",
                    Value = activeAlertsCount.ToString(),
                    Subtitle = "Require operator attention"
                },
                new()
                {
                    Title = "Data Flow",
                    Value = liveTagsCount > 0 ? "OK" : "NO DATA",
                    Subtitle = "MQTT, Redis and API status"
                }
            },

            SystemCards = new List<SystemCardDto>
            {
                new()
                {
                    Label = "Collector Service",
                    Status = liveTagsCount > 0 ? "Online" : "Unknown"
                },
                new()
                {
                    Label = "MQTT Broker",
                    Status = liveTagsCount > 0 ? "Online" : "Unknown"
                },
                new()
                {
                    Label = "TimescaleDB",
                    Status = liveTagsCount > 0 ? "Online" : "Unknown"
                },
                new()
                {
                    Label = "Redis Cache",
                    Status = liveTagsCount > 0 ? "Online" : "Offline"
                }
            },

            RecentAlerts = latestReadings
                .Where(x => !string.Equals(x.Quality, "Good", StringComparison.OrdinalIgnoreCase))
                .Take(5)
                .Select(x => new RecentAlertDto
                {
                    Title = $"Tag quality issue: {x.TagName}",
                    Source = x.Source
                })
                .ToList(),

            TopTags = topTags
        };
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

    private List<TagRedisReading> GetLatestReadingsFromRedis()
    {
        var results = new List<TagRedisReading>();

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

            if (reading is not null)
            {
                results.Add(reading);
            }
        }

        return results
            .OrderBy(x => x.TagName)
            .ToList();
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

        if (quotedParts.Count >= 2)
        {
            return quotedParts[1];
        }

        if (quotedParts.Count == 1)
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