using IndustrialMonitoring.Api.Models.OpcUa;
using IndustrialMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitoring.Api.Controllers;

[ApiController]
[Route("api/opcua")]
public class OpcUaController : ControllerBase
{
    private readonly IOpcUaDiscoveryService _opcUaDiscoveryService;

    public OpcUaController(IOpcUaDiscoveryService opcUaDiscoveryService)
    {
        _opcUaDiscoveryService = opcUaDiscoveryService;
    }

    [HttpGet("sections")]
    public IActionResult GetSections()
    {
        var result = _opcUaDiscoveryService.GetSections();
        return Ok(result);
    }

    [HttpGet("sections/{id:int}/tags")]
    public IActionResult GetTagsBySectionId(int id)
    {
        var result = _opcUaDiscoveryService.GetTagsBySectionId(id);
        return Ok(result);
    }

    [HttpPut("sections/{id:int}/enabled")]
    public IActionResult UpdateSectionEnabledState(int id, [FromBody] UpdateEnabledStateRequest request)
    {
        _opcUaDiscoveryService.UpdateSectionEnabledState(id, request.IsEnabled);
        return NoContent();
    }

    [HttpPut("tags/{id:int}/enabled")]
    public IActionResult UpdateTagEnabledState(int id, [FromBody] UpdateEnabledStateRequest request)
    {
        _opcUaDiscoveryService.UpdateTagEnabledState(id, request.IsEnabled);
        return NoContent();
    }

    [HttpPut("sections/{id:int}/tags/enabled")]
    public IActionResult UpdateTagsEnabledStateBySection(int id, [FromBody] UpdateEnabledStateRequest request)
    {
        _opcUaDiscoveryService.UpdateTagsEnabledStateBySection(id, request.IsEnabled);
        return NoContent();
    }
}