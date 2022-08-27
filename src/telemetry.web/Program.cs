
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using MassTransit;
using OpenTelemetry.Extensions.Propagators;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

// Define some important constants to initialize tracing with
var serviceName = "telemetry_web";
var serviceVersion = typeof(Program).Assembly.GetName().Version.ToString();

var builder = WebApplication.CreateBuilder(args);

Sdk.SetDefaultTextMapPropagator(new CompositeTextMapPropagator(new TextMapPropagator[]
{
    new TraceContextPropagator(),
    new JaegerPropagator()
}));

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
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddSqlClientInstrumentation()
    .AddJaegerExporter(o => {
        o.AgentHost = "jaeger";
    });
});

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
