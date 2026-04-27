namespace IndustrialMonitoring.StorageConsumer.Configurations;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string KeyPrefix { get; set; } = "industrial:latest:";
}