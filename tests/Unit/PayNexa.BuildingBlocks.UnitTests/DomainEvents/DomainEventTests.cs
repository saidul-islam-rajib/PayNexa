using Mediator;
using Microsoft.Extensions.DependencyInjection;
using PayNexa.Common.Behaviors;
using PayNexa.Common.DomainEvents;
using PayNexa.SharedKernel.Domain;
using PayNexa.SharedKernel.Results;

namespace PayNexa.BuildingBlocks.UnitTests.DomainEvents;

public sealed class DomainEventTests
{
    public sealed record SampleHappened(DateTime OccurredAtUtc) : DomainEvent(OccurredAtUtc);

    public sealed class RecordingHandler : IDomainEventHandler<SampleHappened>
    {
        public List<SampleHappened> Handled { get; } = [];

        public Task HandleAsync(SampleHappened domainEvent, CancellationToken cancellationToken)
        {
            Handled.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    public sealed record SampleCommand : ICommand<Result<string>>;

    [Fact]
    public async Task Dispatcher_InvokesHandlersDiscoveredFromTheAssembly()
    {
        var services = new ServiceCollection().AddLogging();
        services.AddDomainEventHandlers(typeof(DomainEventTests).Assembly);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        var domainEvent = new SampleHappened(DateTime.UtcNow);
        await scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>()
            .DispatchAsync([domainEvent], TestContext.Current.CancellationToken);

        var handler = (RecordingHandler)scope.ServiceProvider.GetRequiredService<IDomainEventHandler<SampleHappened>>();
        handler.Handled.ShouldBe([domainEvent]);
    }

    [Fact]
    public void DomainEvent_HasStableIdentity()
    {
        var domainEvent = new SampleHappened(DateTime.UtcNow);

        domainEvent.EventId.ShouldNotBe(Guid.Empty);
        domainEvent.EventId.ShouldBe(domainEvent.EventId);
    }

    [Fact]
    public async Task DomainRuleBehavior_ConvertsDomainExceptionIntoFailedResult()
    {
        var error = Error.BusinessRule("Sample.Rule", "Rule violated.");

        var result = await new DomainRuleBehavior<SampleCommand, Result<string>>().Handle(
            new SampleCommand(),
            (_, _) => throw new DomainException(error),
            TestContext.Current.CancellationToken);

        result.Error.ShouldBe(error);
    }
}
