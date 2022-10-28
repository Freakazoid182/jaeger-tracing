using MassTransit;
using telemetry.worker.Consumers;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Extensions.Docker.Resources;
using telemetry.worker.Internal;
using telemetry.worker.Data;
using telemetry.infrastructure.Elasticsearch;
using System.Reflection;

// Define some important constants to initialize tracing with
var serviceName = "telemetry_worker";
var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString();

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry
var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName, serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName);

// Configure important OpenTelemetry settings, the console exporter, and instrumentation library
builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
    .AddSource(serviceName)
    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName)
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
    .AddOtlpExporter(opt =>
    {
        opt.Endpoint = new Uri("http://otel-col:4317");
        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    });
});

builder.Services.AddOpenTelemetryMetrics(builder => builder
    .ConfigureResource(configureResource)
    .AddRuntimeInstrumentation()
    .AddAspNetCoreInstrumentation()
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddDetector(new DockerResourceDetector()))
    .AddOtlpExporter(opt =>
    {
        opt.Endpoint = new Uri("http://otel-col:4317");
        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    }
));

builder.Services.AddOpenTelemetryMetrics(builder => builder
    .ConfigureResource(configureResource)
    .AddRuntimeInstrumentation()
    .AddAspNetCoreInstrumentation()
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddDetector(new DockerResourceDetector()))
    .AddOtlpExporter(opt =>
    {
        opt.Endpoint = new Uri("http://otel-col:4317");
        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    }
));

// Logging
builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry(options =>
{
    options.ConfigureResource(configureResource);
    options.AddConsoleExporter();
    options.AddOtlpExporter(opt =>
    {
        opt.Endpoint = new Uri("http://otel-col:4317");
        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    });
});

builder.Services.Configure<OpenTelemetryLoggerOptions>(opt =>
{
    opt.IncludeScopes = true;
    opt.ParseStateValues = true;
    opt.IncludeFormattedMessage = true;
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
