namespace IndustrialMonitoring.Collector.Configurations;

public class MqttSettings
{
    public string BrokerHost { get; set; } = "localhost";
    public int BrokerPort { get; set; } = 1883;
    public string Topic { get; set; } = "industrial/tags";
    public string ClientId { get; set; } = "collector-service";
}