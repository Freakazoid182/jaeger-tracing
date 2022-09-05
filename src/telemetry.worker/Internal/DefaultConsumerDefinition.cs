namespace telemetry.worker.Internal;

using MassTransit;

class DefaultConsumerDefinition<TConsumer> : ConsumerDefinition<TConsumer> where TConsumer : class, IConsumer
{
    public DefaultConsumerDefinition()
    {
        // override the default endpoint name
        EndpointName = "telemetry-worker";

        // limit the number of messages consumed concurrently
        // this applies to the consumer only, not the endpoint
        ConcurrentMessageLimit = 8;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TConsumer> consumerConfigurator)
    {
    }
}