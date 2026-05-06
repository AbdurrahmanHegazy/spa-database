namespace IndustrialMonitoring.Collector.Configurations;

public class CollectorSettings
{
    public int ReadIntervalSeconds { get; set; } = 10;
    public bool RunDiscoveryOnStartup { get; set; } = true;
    public bool RunSampling { get; set; } = true;
}