namespace IndustrialMonitoring.Shared.Models;

public class TagReading
{
    public string TagName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Quality { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}