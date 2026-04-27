using IndustrialMonitoring.Shared.Models;
    
namespace IndustrialMonitoring.Collector.OpcUa;

public class OpcUaClientService : IOpcUaClient
{
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Fake OPC UA connection established.");
        return Task.CompletedTask;
    }

    public Task<TagReading?> ReadTagAsync(string tagName, CancellationToken cancellationToken)
    {
        var reading = new TagReading
        {
            TagName = tagName,
            Value = "123.45",
            Timestamp = DateTime.UtcNow,
            Quality = "Good",
            Source = "WinCC Unified"
        };

        return Task.FromResult<TagReading?>(reading);
    }
}