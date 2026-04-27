using IndustrialMonitoring.Collector.Configurations;
using IndustrialMonitoring.Collector.OpcUa;
using Microsoft.Extensions.Options;

namespace IndustrialMonitoring.Collector.Services;

public class CollectorWorker : BackgroundService
{
    private readonly ILogger<CollectorWorker> _logger;
    private readonly IOpcUaClient _opcUaClient;
    private readonly IMqttPublisherService _mqttPublisherService;
    private readonly OpcUaSettings _opcUaSettings;
    private readonly CollectorSettings _collectorSettings;

    public CollectorWorker(
        ILogger<CollectorWorker> logger,
        IOpcUaClient opcUaClient,
        IMqttPublisherService mqttPublisherService,
        IOptions<OpcUaSettings> opcUaOptions,
        IOptions<CollectorSettings> collectorOptions)
    {
        _logger = logger;
        _opcUaClient = opcUaClient;
        _mqttPublisherService = mqttPublisherService;
        _opcUaSettings = opcUaOptions.Value;
        _collectorSettings = collectorOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Collector started.");

        await _opcUaClient.ConnectAsync(stoppingToken);

        if (_opcUaSettings.Tags.Count == 0)
        {
            _logger.LogWarning("No tags were found in configuration.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting reading cycle at: {Time}", DateTime.Now);

            foreach (string tagName in _opcUaSettings.Tags)
            {
                _logger.LogInformation("Reading configured tag: {TagName}", tagName);

                var reading = await _opcUaClient.ReadTagAsync(tagName, stoppingToken);

                if (reading is not null)
                {
                    _logger.LogInformation(
                        "Tag: {TagName}, Value: {Value}, Timestamp: {Timestamp}, Quality: {Quality}, Source: {Source}",
                        reading.TagName,
                        reading.Value,
                        reading.Timestamp,
                        reading.Quality,
                        reading.Source
                    );

                    await _mqttPublisherService.PublishAsync(reading, stoppingToken);
                }
                else
                {
                    _logger.LogWarning("No reading returned for tag: {TagName}", tagName);
                }
            }

            _logger.LogInformation(
                "Reading cycle completed. Waiting {Seconds} seconds...",
                _collectorSettings.ReadIntervalSeconds
            );

            await Task.Delay(TimeSpan.FromSeconds(_collectorSettings.ReadIntervalSeconds), stoppingToken);
        }
    }
}