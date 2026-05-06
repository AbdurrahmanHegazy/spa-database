using IndustrialMonitoring.Collector.Configurations;
using IndustrialMonitoring.Collector.OpcUa;
using IndustrialMonitoring.Collector.Storage;
using Microsoft.Extensions.Options;

namespace IndustrialMonitoring.Collector.Services;

public class OpcUaDiscoveryWorker : BackgroundService
{
    private readonly ILogger<OpcUaDiscoveryWorker> _logger;
    private readonly IOpcUaClient _opcUaClient;
    private readonly OpcUaDiscoveryPersistenceService _discoveryPersistenceService;
    private readonly CollectorSettings _collectorSettings;

    public OpcUaDiscoveryWorker(
        ILogger<OpcUaDiscoveryWorker> logger,
        IOpcUaClient opcUaClient,
        OpcUaDiscoveryPersistenceService discoveryPersistenceService,
        IOptions<CollectorSettings> collectorOptions)
    {
        _logger = logger;
        _opcUaClient = opcUaClient;
        _discoveryPersistenceService = discoveryPersistenceService;
        _collectorSettings = collectorOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_collectorSettings.RunDiscoveryOnStartup)
        {
            _logger.LogInformation("Discovery worker disabled by configuration.");
            return;
        }

        _logger.LogInformation("Discovery worker started.");

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
        else
        {
            _logger.LogWarning("Discovery completed but no section was returned.");
        }

        _logger.LogInformation("Discovery worker completed.");
    }
}