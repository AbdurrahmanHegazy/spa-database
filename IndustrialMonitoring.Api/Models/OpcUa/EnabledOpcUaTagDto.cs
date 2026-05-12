namespace IndustrialMonitoring.Api.Models.OpcUa;

public class EnabledOpcUaTagDto
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}