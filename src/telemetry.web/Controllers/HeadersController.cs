using Microsoft.AspNetCore.Mvc;

namespace telemetry.Controllers;

[ApiController]
[Route("[controller]")]
public class HeadersController : ControllerBase
{

    private readonly ILogger<HeadersController> _logger;
    public HeadersController(
        ILogger<HeadersController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetHeaders")]
    public IHeaderDictionary Get()
    {
        return HttpContext.Request.Headers;
    }
}
