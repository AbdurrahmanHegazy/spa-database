using System.Text.Json;
using IndustrialMonitoring.Shared.Models;
using IndustrialMonitoring.StorageConsumer.Configurations;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace IndustrialMonitoring.StorageConsumer.Storage;

public class RedisRepository : IRedisRepository
{
    private readonly RedisSettings _redisSettings;
    private readonly ILogger<RedisRepository> _logger;

    public RedisRepository(
        IOptions<RedisSettings> redisOptions,
        ILogger<RedisRepository> logger)
    {
        _redisSettings = redisOptions.Value;
        _logger = logger;
    }

    public async Task SaveLatestAsync(TagReading reading)
    {
        try
        {
            await using var connection = await ConnectionMultiplexer.ConnectAsync(_redisSettings.ConnectionString);
            var db = connection.GetDatabase();

            string key = $"{_redisSettings.KeyPrefix}{reading.TagName}";
            string value = JsonSerializer.Serialize(reading);

            await db.StringSetAsync(key, value);

            _logger.LogInformation("Saved latest value to Redis for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving latest value to Redis.");
        }
    }
}