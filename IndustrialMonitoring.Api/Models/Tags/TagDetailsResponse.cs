namespace IndustrialMonitoring.Api.Models.Tags;

public class TagDetailsResponse
{
    public string DisplayName { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string MqttTopic { get; set; } = string.Empty;
    public string RedisKey { get; set; } = string.Empty;
    public string CurrentValue { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string LastUpdate { get; set; } = string.Empty;
    public string DeviceState { get; set; } = string.Empty;
    public string Minimum { get; set; } = string.Empty;
    public string Maximum { get; set; } = string.Empty;
    public string Average { get; set; } = string.Empty;
    public string Samples { get; set; } = string.Empty;
}
