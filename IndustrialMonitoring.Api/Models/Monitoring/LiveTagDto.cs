namespace IndustrialMonitoring.Api.Models.Monitoring;

public class LiveTagDto
{
    public string Id { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string Freshness { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}