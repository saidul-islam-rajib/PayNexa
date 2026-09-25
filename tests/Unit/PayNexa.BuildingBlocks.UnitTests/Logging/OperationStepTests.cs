using PayNexa.Common.Logging;

namespace PayNexa.BuildingBlocks.UnitTests.Logging;

public sealed class OperationStepTests
{
    private readonly FakeLogger _logger = new();

    [Fact]
    public void Succeeded_LogsStartedSucceededEnded()
    {
        using (OperationContext.Begin("CreateCustomerCommand"))
        using (var step = _logger.BeginStep("Email uniqueness check"))
        {
            step.Succeeded();
        }

        var messages = _logger.Collector.GetSnapshot().Select(record => record.Message).ToArray();

        messages.Length.ShouldBe(3);
        messages[0].ShouldBe("CreateCustomerCommand: Email uniqueness check started");
        messages[1].ShouldStartWith("CreateCustomerCommand: Email uniqueness check succeeded in ");
        messages[2].ShouldBe("CreateCustomerCommand: Email uniqueness check ended");
    }

    [Fact]
    public void Failed_LogsWarningWithReason()
    {
        using (var step = _logger.BeginStep("Email uniqueness check"))
        {
            step.Failed("Email already registered");
        }

        var failure = _logger.Collector.GetSnapshot()[1];
        failure.Level.ShouldBe(LogLevel.Warning);
        failure.Message.ShouldContain("Reason: Email already registered");
    }

    [Fact]
    public void Dispose_WithoutOutcome_LogsFailure()
    {
        using (_logger.BeginStep("Persist"))
        {
        }

        var records = _logger.Collector.GetSnapshot();
        records.Count.ShouldBe(3);
        records[1].Level.ShouldBe(LogLevel.Warning);
        records[1].Message.ShouldContain("without an explicit outcome");
    }

    [Fact]
    public void Outcome_IsRecordedOnlyOnce()
    {
        using (var step = _logger.BeginStep("Persist"))
        {
            step.Succeeded();
            step.Failed("ignored");
        }

        _logger.Collector.Count.ShouldBe(3);
    }
}
