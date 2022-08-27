using MassTransit;
using telemetry.contracts;

namespace telemetry.worker.Consumers;

class TestConsumer :
    IConsumer<TestMessage>
{
    readonly ILogger<TestConsumer> _logger;

    public TestConsumer(ILogger<TestConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<TestMessage> context)
    {
        _logger.LogInformation("Test Submitted: {TestId}", context.Message.MessageId);

        return Task.CompletedTask;
    }
}

class TestConsumerDefinition :
        ConsumerDefinition<TestConsumer>
{
    public TestConsumerDefinition()
    {
        // override the default endpoint name
        EndpointName = "test-service";

        // limit the number of messages consumed concurrently
        // this applies to the consumer only, not the endpoint
        ConcurrentMessageLimit = 8;
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TestConsumer> consumerConfigurator)
    {
        // configure message retry with millisecond intervals
        endpointConfigurator.UseMessageRetry(r => r.Intervals(100,200,500,800,1000));
    }
}