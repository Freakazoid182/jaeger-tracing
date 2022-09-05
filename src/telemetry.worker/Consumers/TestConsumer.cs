using MassTransit;
using telemetry.contracts;
using telemetry.worker.Internal;

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
        _logger.TestSubmitted(context.Message.MessageId);

        return Task.CompletedTask;
    }
}
