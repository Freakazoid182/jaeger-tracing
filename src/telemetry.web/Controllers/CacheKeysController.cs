using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace telemetry.Controllers;

[ApiController]
[Route("[controller]")]
public class CacheKeysController : ControllerBase
{
     private readonly ILogger<CacheKeysController> _logger;
    private readonly IDistributedCache _cache;

    public CacheKeysController(
        ILogger<CacheKeysController> logger,
        IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get([FromRoute]string key)
    {
        var value = await _cache.GetAsync(key);

        if(value == null) return NotFound();

        return Ok(Encoding.UTF8.GetString(value));
    }

    [HttpPost("{key}")]
    public async Task<IActionResult> Post([FromRoute]string key, [FromBody]string value)
    {
        await _cache.SetAsync(key, Encoding.UTF8.GetBytes(value));

        return NoContent();
    }
}
