namespace IndustrialMonitoring.Collector.Configurations;

public class OpcUaSettings
{
    public string EndpointUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSecurity { get; set; }
    public List<string> Tags { get; set; } = new();
}