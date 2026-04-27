using IndustrialMonitoring.Api.Models.Tags;

namespace IndustrialMonitoring.Api.Services;

public interface ITagDetailsService
{
    TagDetailsResponse GetTagDetails(string tagId);
}