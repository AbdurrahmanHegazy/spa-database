using IndustrialMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertsService _alertsService;

    public AlertsController(IAlertsService alertsService)
    {
        _alertsService = alertsService;
    }

    [HttpGet]
    public IActionResult GetAlerts()
    {
        var result = _alertsService.GetAlertsData();
        return Ok(result);
    }
}