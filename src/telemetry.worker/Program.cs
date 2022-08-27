using MassTransit;
using telemetry.worker.Consumers;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry.Context.Propagation;

// Define some important constants to initialize tracing with
var serviceName = "telemetry_worker";
var serviceVersion = "1.0.0";

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
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddJaegerExporter(o => {
        o.AgentHost = "jaeger";
    });
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<TestConsumer>(typeof(TestConsumerDefinition));

    x.UsingRabbitMq((context,cfg) =>
    {
        cfg.Host("rabbitmq", "/", h => {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.Run();
