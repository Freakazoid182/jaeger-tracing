using MassTransit;
using Nest;
using telemetry.contracts;
using telemetry.worker.Data;
using telemetry.worker.Models;

namespace telemetry.worker.Consumers;

class InsertProductConsumer :
    IConsumer<CreateProduct>
{

    readonly ILogger<InsertProductConsumer> _logger;
    private readonly IProductsRepository _productsRepository;
    private readonly IElasticClient _elasticClient;

    public InsertProductConsumer(ILogger<InsertProductConsumer> logger, IProductsRepository productsRepository, IElasticClient elasticClient)
    {
        _logger = logger;
        _productsRepository = productsRepository;
        _elasticClient = elasticClient;
    }

    public async Task Consume(ConsumeContext<CreateProduct> context)
    {
        if(context.Message.Name == null) throw new InvalidOperationException($"nameof(context.Message.Name) is null.");
        var product = new Product(context.Message.Id, context.Message.Name, DateTimeOffset.UtcNow);
        await _productsRepository.InsertAsync(product);

        await _elasticClient.IndexDocumentAsync<Product>(product);
    }
}
