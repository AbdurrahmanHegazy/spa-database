using IndustrialMonitoring.Collector.Configurations;
using IndustrialMonitoring.Collector.OpcUa;
using Microsoft.Extensions.Options;

namespace IndustrialMonitoring.Collector.Services;

public class CollectorWorker : BackgroundService
{
    private readonly ILogger<CollectorWorker> _logger;
    private readonly IOpcUaClient _opcUaClient;
    private readonly IMqttPublisherService _mqttPublisherService;
    private readonly EnabledTagsProvider _enabledTagsProvider;
    private readonly CollectorSettings _collectorSettings;

    public CollectorWorker(
        ILogger<CollectorWorker> logger,
        IOpcUaClient opcUaClient,
        IMqttPublisherService mqttPublisherService,
        EnabledTagsProvider enabledTagsProvider,
        IOptions<CollectorSettings> collectorOptions)
    {
        _logger = logger;
        _opcUaClient = opcUaClient;
        _mqttPublisherService = mqttPublisherService;
        _enabledTagsProvider = enabledTagsProvider;
        _collectorSettings = collectorOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_collectorSettings.RunSampling)
        {
            _logger.LogInformation("Sampling worker disabled by configuration.");
            return;
        }

        _logger.LogInformation("Sampling worker started.");

        await _opcUaClient.ConnectAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting reading cycle at: {Time}", DateTime.Now);

            var enabledTagNodeIds = await _enabledTagsProvider.GetEnabledTagNodeIdsAsync(stoppingToken);

            _logger.LogInformation("Enabled tags loaded from database: {Count}", enabledTagNodeIds.Count);

            if (enabledTagNodeIds.Count == 0)
            {
                _logger.LogWarning("No enabled tags found in database.");
            }
            else
            {
                foreach (string tagNodeId in enabledTagNodeIds)
                {
                    _logger.LogInformation("Reading enabled tag: {TagNodeId}", tagNodeId);

                    var reading = await _opcUaClient.ReadTagAsync(tagNodeId, stoppingToken);

                    if (reading is not null)
                    {
                        _logger.LogInformation(
                            "Tag: {TagName}, Value: {Value}, Timestamp: {Timestamp}, Quality: {Quality}, Source: {Source}",
                            reading.TagName,
                            reading.Value,
                            reading.Timestamp,
                            reading.Quality,
                            reading.Source);

                        await _mqttPublisherService.PublishAsync(reading, stoppingToken);
                    }
                    else
                    {
                        _logger.LogWarning("No reading returned for tag: {TagNodeId}", tagNodeId);
                    }
                }
            }

            _logger.LogInformation(
                "Reading cycle completed. Waiting {Seconds} seconds...",
                _collectorSettings.ReadIntervalSeconds);

            await Task.Delay(
                TimeSpan.FromSeconds(_collectorSettings.ReadIntervalSeconds),
                stoppingToken);
        }
    }
}