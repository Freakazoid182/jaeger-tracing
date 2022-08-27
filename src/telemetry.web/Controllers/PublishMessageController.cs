using MassTransit;
using Microsoft.AspNetCore.Mvc;
using telemetry.contracts;

namespace telemetry.Controllers;

[ApiController]
[Route("[controller]")]
public class PublishMessageController : ControllerBase
{
     private readonly ILogger<PublishMessageController> _logger;
    private readonly IBus _bus;

    public PublishMessageController(
        ILogger<PublishMessageController> logger,
        IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    [HttpPost(Name = "PostMessage")]
    public Task Post()
    {
        return _bus.Publish<TestMessage>(new TestMessage { MessageId = Guid.NewGuid() });
    }
}
