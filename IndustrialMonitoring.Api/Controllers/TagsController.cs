using IndustrialMonitoring.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialMonitoring.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagDetailsService _tagDetailsService;

    public TagsController(ITagDetailsService tagDetailsService)
    {
        _tagDetailsService = tagDetailsService;
    }

    [HttpGet("{tagId}")]
    public IActionResult GetTagDetails(string tagId)
    {
        var result = _tagDetailsService.GetTagDetails(tagId);
        return Ok(result);
    }
}