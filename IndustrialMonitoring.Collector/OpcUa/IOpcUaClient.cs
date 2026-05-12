using IndustrialMonitoring.Shared.Models;

namespace IndustrialMonitoring.Collector.OpcUa;

public interface IOpcUaClient
{
    Task<DiscoveredOpcUaSection?> DiscoverSectionAsync(string sectionNodeId, CancellationToken cancellationToken);
    Task ConnectAsync(CancellationToken cancellationToken);
    Task<TagReading?> ReadTagAsync(string tagName, CancellationToken cancellationToken);
    Task BrowseRootAsync(CancellationToken cancellationToken);
    Task SearchAsync(string searchText, CancellationToken cancellationToken);
    Task BrowseNodeAsync(string nodeIdText, CancellationToken cancellationToken);

    Task<DiscoveredOpcUaProject?> DiscoverProjectAsync(string rootNodeId, CancellationToken cancellationToken);

}