using IndustrialMonitoring.StorageConsumer.Services;

namespace IndustrialMonitoring.StorageConsumer.Services;

public class StorageConsumerWorker : BackgroundService
{
    private readonly ILogger<StorageConsumerWorker> _logger;
    private readonly IMqttSubscriberService _mqttSubscriberService;

    public StorageConsumerWorker(
        ILogger<StorageConsumerWorker> logger,
        IMqttSubscriberService mqttSubscriberService)
    {
        _logger = logger;
        _mqttSubscriberService = mqttSubscriberService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StorageConsumer started.");

        await _mqttSubscriberService.StartAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10000, stoppingToken);
        }
    }
}