using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Extensions.Docker.Resources;
using MassTransit;
using telemetry.contracts;
using StackExchange.Redis;
using telemetry.infrastructure.Elasticsearch;

// Define some important constants to initialize tracing with
var serviceName = "telemetry_web";
var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString();

var builder = WebApplication.CreateBuilder(args);

ConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect("redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
builder.Services.AddStackExchangeRedisCache(setup =>
{
    setup.ConnectionMultiplexerFactory = () =>
    {
        return Task.FromResult((IConnectionMultiplexer)connectionMultiplexer);
    };
    setup.InstanceName = "telemetry";
});


builder.Services.AddElasticsearch();

// Configure important OpenTelemetry settings, the console exporter, and instrumentation library
builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
    .AddConsoleExporter()
    .AddSource(serviceName)
    .AddSource("MassTransit")
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddDetector(new DockerResourceDetector()))
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddElasticsearchClientInstrumentation(cfg =>
    {
        if(builder.Environment.IsDevelopment())
        {
            cfg.ParseAndFormatRequest = true;
        }
    })
    .AddRedisInstrumentation(configure: cfg => {
        if(builder.Environment.IsDevelopment())
        {
            cfg.SetVerboseDatabaseStatements = true;
        }
    })
    .AddOtlpExporter(opt =>
    {
        opt.Endpoint = new Uri("http://otel-col:4317");
        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    });
});

builder.Services.AddOpenTelemetryMetrics(builder => builder
    .ConfigureResource(r => r.AddTelemetrySdk())
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


builder.Services.AddMassTransit(x =>
{
    // elided...

    x.UsingRabbitMq((context,cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

EndpointConvention.Map<CreateProduct>(new Uri("rabbitmq://rabbitmq/%2F/telemetry-worker"));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
