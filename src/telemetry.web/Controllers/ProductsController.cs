using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Nest;
using telemetry.contracts;
using telemetry.domain.Models;

namespace telemetry.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
     private readonly ILogger<ProductsController> _logger;
    private readonly IBus _bus;
    private readonly IElasticClient _elasticClient;

    public ProductsController(
        ILogger<ProductsController> logger,
        IBus bus,
        IElasticClient elasticClient)
    {
        _logger = logger;
        _bus = bus;
        _elasticClient = elasticClient;
    }

    [HttpPost]
    public Task PostProduct([FromBody]CreateProduct product)
    {
        return _bus.Send(product);
    }

    [HttpPost("search")]
    public async Task<IActionResult> PostProductSearch([FromBody]string query)
    {
        var result = await _elasticClient.SearchAsync<Product>(p => p.Query(q => q.QueryString(d => d.Query(query))));

        return Ok(result.Documents);
    }
}
