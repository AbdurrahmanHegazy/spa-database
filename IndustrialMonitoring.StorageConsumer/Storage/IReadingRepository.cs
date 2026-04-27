using IndustrialMonitoring.Shared.Models;

namespace IndustrialMonitoring.StorageConsumer.Storage;

public interface IReadingRepository
{
    Task SaveAsync(TagReading reading, CancellationToken cancellationToken);
}