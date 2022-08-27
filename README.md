# Jaeger Tracing with OpenTelemetry and ASP.NET Core

This is an example on how OpenTelemetry can be set up using Jaeger on ASP.NET Core

- Uses envoy proxy as ingress gateway
- Uses [MassTransit](https://masstransit-project.com/) and [RabbitMQ](https://www.rabbitmq.com/) as for messaing
- Traces are correlated end to end using [JaegerPropagator](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Extensions.Propagators/JaegerPropagator.cs)

# Running the solution

```sh
docker-compose up -d --build
```

Jaeger: http://localhost:16686

RabbitMq: http://localhost:15672 (guest/guest)

Telemetry Web Swagger (through Envoy proxy): http://localhost:10000/swagger/index.html

----

![Full end to end](/images/Screenshot1.png)
