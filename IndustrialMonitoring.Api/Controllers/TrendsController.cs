using IndustrialMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrendsController : ControllerBase
{
    private readonly ITrendsService _trendsService;

    public TrendsController(ITrendsService trendsService)
    {
        _trendsService = trendsService;
    }

    [HttpGet]
    public IActionResult GetTrends(
        [FromQuery] string? tagId,
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? timeRange)
    {
        var result = _trendsService.GetTrendsData(tagId, from, to, timeRange);
        return Ok(result);
    }
}