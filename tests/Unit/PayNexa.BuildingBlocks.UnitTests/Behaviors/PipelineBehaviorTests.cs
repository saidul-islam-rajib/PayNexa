using FluentValidation;
using Mediator;
using Microsoft.Extensions.Options;
using PayNexa.Common.Behaviors;
using PayNexa.Common.Exceptions;
using PayNexa.SharedKernel.Results;

namespace PayNexa.BuildingBlocks.UnitTests.Behaviors;

public sealed class PipelineBehaviorTests
{
    public sealed record SampleCommand(string Name) : ICommand<Result<string>>;

    private sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator() => RuleFor(command => command.Name).NotEmpty();
    }

    [Fact]
    public async Task Validation_InvalidMessage_ReturnsValidationFailureWithoutCallingHandler()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<string>>([new SampleCommandValidator()], new FakeLogger<ValidationBehavior<SampleCommand, Result<string>>>());
        var handlerCalled = false;

        var result = await behavior.Handle(new SampleCommand(""), (_, _) =>
        {
            handlerCalled = true;
            return ValueTask.FromResult<Result<string>>("ok");
        }, TestContext.Current.CancellationToken);

        handlerCalled.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error!.Type.ShouldBe(ErrorType.Validation);
        result.Error.ValidationErrors!.ShouldContainKey(nameof(SampleCommand.Name));
    }

    [Fact]
    public async Task Validation_ValidMessage_CallsHandler()
    {
        var behavior = new ValidationBehavior<SampleCommand, Result<string>>([new SampleCommandValidator()], new FakeLogger<ValidationBehavior<SampleCommand, Result<string>>>());

        var result = await behavior.Handle(new SampleCommand("valid"), (_, _) => ValueTask.FromResult<Result<string>>("ok"), TestContext.Current.CancellationToken);

        result.Value.ShouldBe("ok");
    }

    [Fact]
    public async Task Logging_Success_LogsStartedSucceededEnded()
    {
        var logger = new FakeLogger<LoggingBehavior<SampleCommand, Result<string>>>();
        var behavior = CreateLoggingBehavior(logger);

        await behavior.Handle(new SampleCommand("valid"), (_, _) => ValueTask.FromResult<Result<string>>("ok"), TestContext.Current.CancellationToken);

        var messages = logger.Collector.GetSnapshot().Select(record => record.Message).ToArray();
        messages[0].ShouldBe("Command SampleCommand started");
        messages[1].ShouldStartWith("SampleCommand succeeded in ");
        messages[^1].ShouldBe("SampleCommand ended");
    }

    [Fact]
    public async Task Logging_ExpectedFailure_LogsWarningNotError()
    {
        var logger = new FakeLogger<LoggingBehavior<SampleCommand, Result<string>>>();
        var behavior = CreateLoggingBehavior(logger);

        await behavior.Handle(
            new SampleCommand("valid"),
            (_, _) => ValueTask.FromResult<Result<string>>(Error.NotFound("Sample.NotFound", "missing")),
            TestContext.Current.CancellationToken);

        var failure = logger.Collector.GetSnapshot().Single(record => record.Message.Contains("failed"));
        failure.Level.ShouldBe(LogLevel.Warning);
        failure.Message.ShouldContain("Sample.NotFound");
    }

    [Fact]
    public async Task Logging_UnhandledException_LogsErrorAndMarksExceptionAsLogged()
    {
        var logger = new FakeLogger<LoggingBehavior<SampleCommand, Result<string>>>();
        var behavior = CreateLoggingBehavior(logger);

        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new SampleCommand("valid"), (_, _) => throw new InvalidOperationException("boom"), TestContext.Current.CancellationToken));

        exception.IsLogged().ShouldBeTrue();
        logger.Collector.GetSnapshot().ShouldContain(record => record.Level == LogLevel.Error && record.Exception == exception);
        logger.Collector.LatestRecord.Message.ShouldBe("SampleCommand ended");
    }

    private static LoggingBehavior<SampleCommand, Result<string>> CreateLoggingBehavior(ILogger<LoggingBehavior<SampleCommand, Result<string>>> logger) =>
        new(logger, Options.Create(new OperationLoggingOptions { SlowOperationThresholdMs = 10_000 }));
}
