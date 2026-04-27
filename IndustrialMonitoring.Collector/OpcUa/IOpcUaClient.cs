using IndustrialMonitoring.Shared.Models;


namespace IndustrialMonitoring.Collector.OpcUa;

public interface IOpcUaClient
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task<TagReading?> ReadTagAsync(string tagName, CancellationToken cancellationToken);
}