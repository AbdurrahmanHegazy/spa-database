using IndustrialMonitoring.Collector.Configurations;
using IndustrialMonitoring.Collector.OpcUa;
using IndustrialMonitoring.Collector.Storage;
using Microsoft.Extensions.Options;

namespace IndustrialMonitoring.Collector.Services;

public class CollectorWorker : BackgroundService
{
    private readonly ILogger<CollectorWorker> _logger;
    private readonly IOpcUaClient _opcUaClient;
    private readonly IMqttPublisherService _mqttPublisherService;
    private readonly OpcUaDiscoveryPersistenceService _discoveryPersistenceService;
    private readonly EnabledTagsProvider _enabledTagsProvider;
    private readonly CollectorSettings _collectorSettings;

    public CollectorWorker(
        ILogger<CollectorWorker> logger,
        IOpcUaClient opcUaClient,
        IMqttPublisherService mqttPublisherService,
        IOptions<CollectorSettings> collectorOptions,
        OpcUaDiscoveryPersistenceService discoveryPersistenceService,
        EnabledTagsProvider enabledTagsProvider)
    {
        _logger = logger;
        _opcUaClient = opcUaClient;
        _mqttPublisherService = mqttPublisherService;
        _collectorSettings = collectorOptions.Value;
        _discoveryPersistenceService = discoveryPersistenceService;
        _enabledTagsProvider = enabledTagsProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Collector started.");

        await _opcUaClient.ConnectAsync(stoppingToken);

        var discoveredSection = await _opcUaClient.DiscoverSectionAsync(
            "ns=3;s=\"Sensori analogici\"",
            stoppingToken);

        if (discoveredSection is not null)
        {
            await _discoveryPersistenceService.SaveSectionAsync(discoveredSection, stoppingToken);

            _logger.LogInformation(
                "Discovered section {SectionName} with {TagCount} tags.",
                discoveredSection.DisplayName,
                discoveredSection.Tags.Count);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting reading cycle at: {Time}", DateTime.Now);

            var enabledTagNodeIds = await _enabledTagsProvider.GetEnabledTagNodeIdsAsync(stoppingToken);

            _logger.LogInformation(
                "Enabled tags loaded from database: {Count}",
                enabledTagNodeIds.Count);

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