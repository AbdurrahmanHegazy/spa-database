namespace IndustrialMonitoring.Api.Models.Tags;

public class TagDetailsResponse
{
    public string RouteParam { get; set; } = string.Empty;
    public TagSummaryDto Summary { get; set; } = new();
    public List<TagMetadataItemDto> Metadata { get; set; } = new();
    public List<TagStatisticItemDto> Statistics { get; set; } = new();
}

public class TagSummaryDto
{
    public string CurrentValue { get; set; } = "--";
    public string Unit { get; set; } = string.Empty;
    public string Quality { get; set; } = "Unknown";
    public string LastUpdate { get; set; } = "--";
    public string Freshness { get; set; } = "--";
    public string DeviceState { get; set; } = "Unknown";
}

public class TagMetadataItemDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class TagStatisticItemDto
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}