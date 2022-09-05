using MassTransit;
using Microsoft.AspNetCore.Mvc;
using telemetry.contracts;

namespace telemetry.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
     private readonly ILogger<ProductsController> _logger;
    private readonly IBus _bus;

    public ProductsController(
        ILogger<ProductsController> logger,
        IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    [HttpPost]
    public Task PostProduct([FromBody]CreateProduct product)
    {
        return _bus.Send(product);
    }
}
