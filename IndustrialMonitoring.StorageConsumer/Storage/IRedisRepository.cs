using IndustrialMonitoring.Shared.Models;

namespace IndustrialMonitoring.StorageConsumer.Storage;

public interface IRedisRepository
{
    Task SaveLatestAsync(TagReading reading);
}