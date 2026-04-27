namespace IndustrialMonitoring.StorageConsumer.Services;

public interface IMqttSubscriberService
{
    Task StartAsync(CancellationToken cancellationToken);
}