using IndustrialMonitoring.Api.Models.OpcUa;

namespace IndustrialMonitoring.Api.Services;

public interface IOpcUaDiscoveryService
{
    List<OpcUaSectionDto> GetSections();
    List<OpcUaTagDto> GetTagsBySectionId(int sectionId);

    void UpdateSectionEnabledState(int sectionId, bool isEnabled);
    void UpdateTagEnabledState(int tagId, bool isEnabled);

    void UpdateTagsEnabledStateBySection(int sectionId, bool isEnabled);
}