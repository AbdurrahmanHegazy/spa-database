using System.Globalization;
using System.Text.Json;
using IndustrialMonitoring.Api.Models.Tags;
using Microsoft.Extensions.Configuration;
using Npgsql;
using StackExchange.Redis;
using IndustrialMonitoring.Api.Helpers;

namespace IndustrialMonitoring.Api.Services;

public class TagDetailsService : ITagDetailsService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redis;
    private readonly string _connectionString;
    private const string RedisKeyPrefix = "industrial:latest:";

    public TagDetailsService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redisConnection = redis;
        _redis = redis.GetDatabase();
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is missing.");
    }

    public TagDetailsResponse GetTagDetails(string tagId)
    {
        var resolvedTagName = ResolveTagNameFromRoute(tagId);

        var latestReading = GetLatestReadingFromRedis(resolvedTagName);
        var stats = GetLastHourStats(resolvedTagName);

        var displayName = FormatTagDisplayName(resolvedTagName);
        var groupName = ExtractGroupName(resolvedTagName);
        var latestUtcTimestamp = ParseStoredUtcTimestamp(latestReading?.Timestamp);
        var freshness = latestUtcTimestamp is null ? "--" : FormatFreshness(latestUtcTimestamp.Value);
        var currentValue = latestReading?.Value ?? "--";
        var quality = latestReading?.Quality ?? "Unknown";
        var source = latestReading?.Source ?? "Unavailable";
        var lastUpdate = latestUtcTimestamp is null
            ? "--"
            : TimeZoneHelper.UtcToRome(latestUtcTimestamp.Value).ToString("dd/MM/yyyy HH:mm:ss");

        return new TagDetailsResponse
        {
            RawTagName = resolvedTagName,
            RouteParam = tagId,

            Summary = new TagSummaryDto
            {
                CurrentValue = currentValue,
                Unit = "Value",
                Quality = quality,
                LastUpdate = lastUpdate,
                Freshness = freshness,
                DeviceState = GetDeviceState(latestReading, latestUtcTimestamp)
            },

            Metadata = new List<TagMetadataItemDto>
            {
                new() { Label = "Tag Name", Value = displayName },
                new() { Label = "Group", Value = groupName },
                new() { Label = "Data Type", Value = InferDataType(currentValue) },
                new() { Label = "Source", Value = source },
                new() { Label = "MQTT Topic", Value = "industrial/tags" },
                new() { Label = "Redis Key", Value = $"{RedisKeyPrefix}{resolvedTagName}" }
            },

            Statistics = new List<TagStatisticItemDto>
            {
                new() { Label = "Minimum (1h)", Value = stats.MinimumText },
                new() { Label = "Maximum (1h)", Value = stats.MaximumText },
                new() { Label = "Average (1h)", Value = stats.AverageText },
                new() { Label = "Samples (1h)", Value = stats.SamplesText }
            }
        };
    }

    private string ResolveTagNameFromRoute(string routeTagId)
    {
        var endpoints = _redisConnection.GetEndPoints();
        if (endpoints.Length == 0)
        {
            return routeTagId;
        }

        var server = _redisConnection.GetServer(endpoints.First());

        foreach (var key in server.Keys(pattern: $"{RedisKeyPrefix}*"))
        {
            var rawKey = key.ToString();
            var fullTagName = rawKey.Replace(RedisKeyPrefix, "");
            var slug = BuildTagId(fullTagName);

            if (slug.Equals(routeTagId, StringComparison.OrdinalIgnoreCase))
            {
                return fullTagName;
            }
        }

        return routeTagId;
    }

    private TagRedisReading? GetLatestReadingFromRedis(string tagName)
    {
        var redisKey = $"{RedisKeyPrefix}{tagName}";
        var rawValue = _redis.StringGet(redisKey);

        if (rawValue.IsNullOrEmpty)
        {
            return null;
        }

        var json = rawValue.ToString();

        if (json.StartsWith("\""))
        {
            json = JsonSerializer.Deserialize<string>(json) ?? json;
        }

        return JsonSerializer.Deserialize<TagRedisReading>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    private TagStatsResult GetLastHourStats(string tagName)
    {
        var fromUtc = DateTime.UtcNow.AddHours(-1);
        var toUtc = DateTime.UtcNow;

        var numericValues = new List<double>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        const string sql = """
            SELECT value
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

            if (double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                numericValues.Add(parsed);
            }
        }

        if (numericValues.Count == 0)
        {
            return new TagStatsResult
            {
                MinimumText = "--",
                MaximumText = "--",
                AverageText = "--",
                SamplesText = "--"
            };
        }

        return new TagStatsResult
        {
            MinimumText = numericValues.Min().ToString("0.##", CultureInfo.InvariantCulture),
            MaximumText = numericValues.Max().ToString("0.##", CultureInfo.InvariantCulture),
            AverageText = numericValues.Average().ToString("0.##", CultureInfo.InvariantCulture),
            SamplesText = numericValues.Count.ToString("N0", CultureInfo.InvariantCulture)
        };
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

    private static string ExtractGroupName(string tagName)
    {
        var quotedParts = GetQuotedParts(tagName);

        if (quotedParts.Count >= 1)
        {
            return quotedParts[0];
        }

        return "Unknown Group";
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

    private static string FormatFreshness(DateTime timestamp)
    {
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

    private static string GetDeviceState(TagRedisReading? reading, DateTime? utcTimestamp)
    {
        if (reading is null || utcTimestamp is null)
        {
            return "Unknown";
        }

        if (!string.Equals(reading.Quality, "Good", StringComparison.OrdinalIgnoreCase))
        {
            return "Warning";
        }

        var age = DateTime.UtcNow - utcTimestamp.Value;
        if (age.TotalSeconds > 30)
        {
            return "Offline";
        }

        return "Online";
    }

    private static string InferDataType(string value)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
        {
            return "Real";
        }

        return "Unknown";
    }
    private static DateTime? ParseStoredUtcTimestamp(string? timestampText)
    {
        if (string.IsNullOrWhiteSpace(timestampText))
        {
            return null;
        }

        if (DateTime.TryParse(
            timestampText,
            null,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
            out var parsed))
        {
            return DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        }

        return null;
    }
    private class TagRedisReading
    {
        public string TagName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public string Quality { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }

    private class TagStatsResult
    {
        public string MinimumText { get; set; } = "--";
        public string MaximumText { get; set; } = "--";
        public string AverageText { get; set; } = "--";
        public string SamplesText { get; set; } = "--";
    }
}