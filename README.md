# Jaeger Tracing with OpenTelemetry and ASP.NET Core

This is an example on how OpenTelemetry can be set up using Jaeger on ASP.NET Core

- Uses envoy proxy as ingress gateway
- Uses [MassTransit](https://masstransit-project.com/) and [RabbitMQ](https://www.rabbitmq.com/) as for messaging
- Traces are correlated end-to-end using [JaegerPropagator](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Extensions.Propagators/JaegerPropagator.cs)

## Running the solution

```sh
docker-compose up -d --build
```

Jaeger: http://localhost:16686

RabbitMq: http://localhost:15672 (guest/guest)

Telemetry Web Swagger (through Envoy proxy): http://localhost:10000/swagger/index.html

__Monitoring service logs:__

```sh
docker logs -f telemetry_web
```
```sh
docker logs -f telemetry_worker
```
```sh
docker logs -f envoy_proxy
```

## Control the sampling rate

Jaeger client libraries ensure traces are always complete (includes all services end-to-end). Sampling decisions are propegated through service executions

[Docs](https://www.jaegertracing.io/docs/1.37/sampling)

On `envoy/front-envoy-jaeger.yaml` configure the sampler

See the [Jaeger docs](https://www.jaegertracing.io/docs/1.37/sampling/#client-sampling-configuration)

__100%:__
```yaml
sampler:
    type: const
    param: 1
```

__10%:__
```yaml
sampler:
    type: probabilistic
    param: 0.1
```

__2 requests per second:__
```yaml
sampler:
    type: ratelimiting
    param: 2.0
```

----

![Full end to end](/images/Screenshot1.png)

![Full end to end SQL](/images/Screenshot2.png)

![Full end to end Redis](/images/Screenshot3.png)
