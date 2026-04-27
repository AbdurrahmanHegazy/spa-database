using IndustrialMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;

    public MonitoringController(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    [HttpGet]
    public IActionResult GetMonitoring()
    {
        var result = _monitoringService.GetMonitoringData();
        return Ok(result);
    }
}