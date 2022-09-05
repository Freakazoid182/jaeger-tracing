using MassTransit;
using telemetry.contracts;
using telemetry.worker.Data;
using telemetry.worker.Models;

namespace telemetry.worker.Consumers;

class InsertProductConsumer :
    IConsumer<CreateProduct>
{

    readonly ILogger<InsertProductConsumer> _logger;
    private readonly IProductsRepository _productsRepository;

    public InsertProductConsumer(ILogger<InsertProductConsumer> logger, IProductsRepository productsRepository)
    {
        _logger = logger;
        _productsRepository = productsRepository;
    }

    public Task Consume(ConsumeContext<CreateProduct> context)
    {
        if(context.Message.Name == null) throw new InvalidOperationException($"nameof(context.Message.Name) is null.");
        return _productsRepository.InsertAsync(new Product(context.Message.Id, context.Message.Name, DateTimeOffset.UtcNow));
    }
}