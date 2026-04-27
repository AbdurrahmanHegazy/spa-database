using IndustrialMonitoring.Shared.Models;

namespace IndustrialMonitoring.Collector.Services;

public interface IMqttPublisherService
{
    Task PublishAsync(TagReading reading, CancellationToken cancellationToken);
}