namespace telemetry.worker.Internal;

public static class LoggerExtensions
{
    private static readonly Action<ILogger, Guid, Exception?> _testSubmitted;

    static LoggerExtensions()
    {
        _testSubmitted = LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1, nameof(TestSubmitted)),
            "Test Submitted: {MessageId}");
    }

    public static void TestSubmitted(this ILogger logger, Guid messageId)
    {
        _testSubmitted(logger, messageId, null);
    }
}
