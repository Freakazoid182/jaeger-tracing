using MassTransit;
using telemetry.worker.Consumers;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Extensions.Docker.Resources;
using telemetry.worker.Internal;
using telemetry.worker.Data;
using telemetry.infrastructure.Elasticsearch;

// Define some important constants to initialize tracing with
var serviceName = "telemetry_worker";
var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString();

Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
{
    new TraceContextPropagator(),
    new JaegerPropagator()
}));

var builder = WebApplication.CreateBuilder(args);

// Configure important OpenTelemetry settings, the console exporter, and instrumentation library
builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
    .AddConsoleExporter()
    .AddSource(serviceName)
    .AddSource("MassTransit")
    .AddElasticsearchClientInstrumentation(cfg =>
    {
        if(builder.Environment.IsDevelopment())
        {
            cfg.ParseAndFormatRequest = true;
        }
    })
    .AddSqlClientInstrumentation(o =>
    {
        if(builder.Environment.IsDevelopment())
        {
            o.SetDbStatementForText = true;
            o.EnableConnectionLevelAttributes = true;
        }
    })
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddDetector(new DockerResourceDetector()))
    .AddJaegerExporter(o => {
        o.AgentHost = "jaeger";
    });
});

builder.Services.AddElasticsearch();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TestConsumer>(typeof(DefaultConsumerDefinition<TestConsumer>));

    x.AddConsumer<InsertProductConsumer>(typeof(DefaultConsumerDefinition<InsertProductConsumer>));

    x.UsingRabbitMq((context,cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddSingleton<IProductsRepository, ProductsRepository>();

var app = builder.Build();

app.Run();
