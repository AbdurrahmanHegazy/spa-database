using IndustrialMonitoring.Api.Models.OpcUa;

namespace IndustrialMonitoring.Api.Services;

public interface IOpcUaDiscoveryService
{
    List<OpcUaSectionDto> GetSections();
    List<OpcUaTagDto> GetTagsBySectionId(int sectionId);
    List<EnabledOpcUaTagDto> GetEnabledTags();

    void UpdateSectionEnabledState(int sectionId, bool isEnabled);
    void UpdateTagEnabledState(int tagId, bool isEnabled);

    void UpdateTagsEnabledStateBySection(int sectionId, bool isEnabled);

    Task<DiscoveredSectionSyncResultDto> SyncDiscoveredSectionsAsync();
    Task<DiscoveredTagSyncResultDto> SyncDiscoveredTagsForSectionAsync(int sectionId);

   
}