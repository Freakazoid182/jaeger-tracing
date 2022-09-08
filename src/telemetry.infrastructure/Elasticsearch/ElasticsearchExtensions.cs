using Microsoft.Extensions.DependencyInjection;
using Nest;
using telemetry.domain.Models;

namespace telemetry.infrastructure.Elasticsearch;

public static class ElasticsearchExtensions
{
    public static void AddElasticsearch(this IServiceCollection services)
    {
        var productsIndex = "products";
        var settings = new ConnectionSettings(new Uri("http://elasticsearch:9200"))
            .DisableDirectStreaming()
            .DefaultIndex(productsIndex);

        AddDefaultMappings(settings, productsIndex);

        var client = new ElasticClient(settings);

        services.AddSingleton<IElasticClient>(client);

        CreateIndex(client, productsIndex);
    }

    private static void AddDefaultMappings(ConnectionSettings settings, string indexName)
    {
        settings.DefaultMappingFor<Product>(m => m.IndexName(indexName));
    }

    private static void CreateIndex(IElasticClient client, string indexName)
    {
        var createIndexResponse = client.Indices.Create(indexName,
            index => index.Map<Product>(x => x.AutoMap())
        );
    }
}
